﻿using Pl_Covid_19_Anmeldung.connection.exceptions;
using Pl_Covid_19_Anmeldung.security;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Pl_Covid_19_Anmeldung.connection
{
    class PLCASocket : IDisposable
    {
        // Client id to indicate that the requesting user is the covid-login
        private const int CLIENT_ID = 0;

        // Reference to the program-logger
        private readonly Logger log;

        // The stream to access any read/write functions
        private readonly NetworkStream stream;
        // Socket/Client to access the network functions
        private readonly TcpClient client;

        // The rsa-encryption manager
        private RSACryptoServiceProvider rsaService;

        // The aes-key and aes-iv for the secure encryption (Will be generated by the remote client)
        private readonly byte[] aesKey = new byte[32], aesIv = new byte[16];

        // How long no data got send
        private long lastDataSendTime;

        // Timeout-time (How many ms to wait until the connection gets killed)
        public long Timeout = 5000;

        // The nonce bytes that are required to be set in order to accept the request
        private readonly byte[] nonceBytes = new byte[8];

        /// <param name="host">The host/ip that shall be requested</param>
        /// <param name="port">The port on which the remote server is running on the host</param>
        /// <param name="privateKey">the rsa-key that is used to encrypt the connection</param>
        /// <exception cref="SocketException">If anything went wrong with the connection</exception>
        /// <exception cref="IOException">If anything went wrong during the handshake</exception>
        public PLCASocket(Logger log,string host,int port,RSAParameters privateKey) 
        {
            // Creates the logger
            this.log = log;

            try
            {
                this.log
                    .Debug("Starting connection to server")
                    .Critical($"Host={host}; Port={port}");

                // Connects to the remote host
                this.client = new TcpClient(host, port);
                this.stream = this.client.GetStream();
            }
            catch (SocketException)
            {
                this.log.Debug("Connection failed, socket closed.");

                // Converts a socket exception to an io-exception for easier handling later
                throw new IOException();
            }

            // Saves the key
            this.rsaService = new RSACryptoServiceProvider();
            // Loads the key
            this.rsaService.ImportParameters(privateKey);

            // Does the handshake
            this.DoHandshake();
        }

        /// <summary>
        /// Does the secure handshake to establish a secure connection with the server
        /// </summary>
        /// <exception cref="HandshakeException">If the returned message coult not be decrypted (Wrong key is given)</exception>
        /// <exception cref="IOException">If anything went wrong with the I/O</exception>
        private void DoHandshake()
        {
            // Stores the received data for the aes key (Still encrypted using the rsa-key)
            byte[] aesBytes = new byte[256];

            try
            {
                // Sends the client-id
                this.stream.WriteByte(CLIENT_ID);

                this.log.Debug("Send client-id (0)");

                // Sends the random nonce
                this.stream.Write(nonceBytes, 0, nonceBytes.Length);

                this.log
                    .Debug("Send nonce")
                    .Critical("Nonce="+string.Join(",", nonceBytes));

                // Receives the aes-data
                for (int i = 0; i < 256; i++)
                    aesBytes[i] = this.ReadByte();

                this.log
                    .Debug("Received aes-secrets")
                    .Critical("Received aes-bytes="+string.Join(",", aesBytes));
            }
            catch
            {
                this.log.Debug("Connection closed (I/O)");

                throw new IOException();
            }

            try
            {
                // Decryptes the bytes
                byte[] decryptedAes = this.rsaService.Decrypt(aesBytes, false);

                this.log
                    .Debug("Decrypted successfull")
                    .Critical("Decrypted aes bytes="+string.Join(",", decryptedAes));

                // Copies the aes key and aes iv
                Array.Copy(decryptedAes, 0, this.aesKey, 0, 32);
                Array.Copy(decryptedAes, 32, this.aesIv, 0, 16);

                this.log.Critical("Key=" + string.Join(",", this.aesKey) + "; Iv=" + string.Join(",", this.aesIv));
            } catch(IOException)
            {
                throw;
            }
            catch
            {
                this.log.Debug("Failed to decrypt");

                // Failed to decrypt the aes-key
                throw new HandshakeException();
            }
        }

        /// <summary>
        /// Reads the next byte from the stream. If no data is available the method waits until eighter a new byte becomes available or the stream timeouted
        /// </summary>
        /// <returns>The next byte</returns>
        /// <exception cref="IOException">If the stream timeouted or anything went wrong with the I/O</exception>
        private byte ReadByte()
        {
            // Resets the timeout
            this.lastDataSendTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            // Waits until data is available
            while (!this.stream.DataAvailable)
            {
                // Waits
                Thread.Sleep(10);
                // Checks if no data has been available for to long
                if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - this.lastDataSendTime > this.Timeout)
                    throw new IOException("Data-stream timeouted");
            }
            // Gets the next byte
            return (byte)this.stream.ReadByte();
        }

        /// <summary>
        /// Encryptes the given data and sends it as a packet to the receiver
        /// </summary>
        /// <param name="data">The raw bytes that shall be send</param>
        /// <exception cref="IOException">If anything went wrong with the I/O</exception>
        public void SendPacket(params byte[] data)
        {
            try
            {
                // Encryptes the data using the received key
                byte[] encrypted = SimpleSecurityUtils.EncryptAES(data,this.aesKey,this.aesIv);

                // Checks if anything failed with the encryption
                if (encrypted == null)
                    throw new Exception("Encryption failed");

                // Sends the length of the packet
                this.stream.WriteByte((byte)encrypted.Length);
                this.stream.WriteByte((byte)(encrypted.Length >> 8));

                // Sends the actual encrypted data
                this.stream.Write(encrypted, 0, encrypted.Length);
                this.stream.Flush();
            }
            catch(Exception e)
            {
                // Converts any exception to an i/o-exception
                throw new IOException(e.Message);
            }
        }

        /// <summary>
        /// Waits for a new packet from the remote client.
        /// </summary>
        /// <returns>The data</returns>
        /// <exception cref="IOException">If anything went wrong with the I/O</exception>
        public byte[] ReceivePacket()
        {
            try
            {
                // Receives the length of the packet
                int len = this.ReadByte() | (this.ReadByte() << 8);

                // Reserves the space to store the data
                byte[] back = new byte[len];

                // Waits for the data
                for (int i = 0; i < len; i++)
                    back[i] = this.ReadByte();

                // Decrypts the data
                byte[] dec = SimpleSecurityUtils.DecryptAES(back,this.aesKey,this.aesIv);

                // Checks if the decryption failed
                if (dec == null)
                    throw new Exception("Data-decryption failed.");

                this.log
                    .Debug("Checking nonce")
                    .Critical($"Received bytes=[{string.Join(",",dec)}}}");

                // Checks if the dec has at least enough bytes for the nonce bytes
                if (dec.Length < this.nonceBytes.Length)
                    throw new IOException("No nonce provided.");

                // Check if the bytes of the nonce match
                for (int i = 0; i < this.nonceBytes.Length; i++)
                    if (this.nonceBytes[i] != dec[i])
                        throw new IOException("Nonce does not match.");

                this.log
                    .Debug("Nonce is correct.");

                // Creates a copy of the packet without the nonce
                byte[] finPkt = new byte[dec.Length - this.nonceBytes.Length];
                // Copies the bytes of the actual packet
                Array.Copy(dec, this.nonceBytes.Length, finPkt, 0, finPkt.Length);

                return finPkt;
            }
            catch (Exception e)
            {
                // Converts any exception to an i/o-exception
                throw new IOException(e.Message);
            }
        }

        /// <summary>
        /// Kills the connection
        /// </summary>
        public void Dispose()
        {
            // Removes the crypto serivce (RSA)
            if (this.rsaService != null)
                this.rsaService.Dispose();

            // Closes the socket
            if (this.client != null)
                this.client.Close();

            // Closes the stream
            if(this.stream != null)
                this.stream.Close();   
        }
    }
}
