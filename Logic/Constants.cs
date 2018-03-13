namespace Livesplit.DarkSouls100Tracker.Logic
{
    public static class Constants
    {
        public const int PROCESS_VM_READ = 0x10;
        public const int TH32CS_SNAPPROCESS = 0x2;
        public const int MEM_COMMIT = 4096;
        public const int MEM_RELEASE = 0x8000;
        public const int PAGE_READWRITE = 4;
        public const int PAGE_EXECUTE_READWRITE = 0x40;
        public const int PROCESS_CREATE_THREAD = (0x2);
        public const int PROCESS_VM_OPERATION = (0x8);
        public const int PROCESS_VM_WRITE = (0x20);
        public const int PROCESS_ALL_ACCESS = 0x1F0FFF;

        // in milliseconds
        public const int Thread_Frequency = 33;
        public const int BONFIRE_FULLY_KINDLED = 40;
    }
}
