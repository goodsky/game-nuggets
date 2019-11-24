namespace GameData
{
    public class SaveInfo
    {
        public SaveInfo(string path, bool isOnDisk = true)
        {
            Path = path;
            IsOnDisk = isOnDisk;
        }

        public string Path { get; private set; }

        public bool IsOnDisk { get; private set; }
    }
}
