﻿namespace projektlabor.noah.planmeldung
{
    static class Config
    {
        // Credentials to the database where all information gets stored
        public static string DB_NAME = "planmeldung";
        public static string DB_PASSWORD = "";
        public static string DB_USERNAME = "root";
        public static string DB_ADDRESS = "127.0.0.1";

        // Credentials for the remote email server to send an email to itself with the credentials
        public static string SMTP_ADDRESS = "smtp.goneo.de";
        public static string SMTP_EMAIL = "someemail@projektlabor.org";
        public static string SMTP_PASSWORD = "SomePassword";
        public static int SMTP_PORT = 587;

        public static string ADMIN_PASSWORD = "1337";

        // How many seconds to wait between backups
        public static int BACKUP_SCHEDULE_SECONDS = 60 * 60 * 12;
    }
}
