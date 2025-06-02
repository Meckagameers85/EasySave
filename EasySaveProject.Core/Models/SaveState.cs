public class SaveState
{
    public string name { get; set; } = "";
    public string sourceFilePath { get; set; } = "";
    public string targetFilePath { get; set; } = "";
    public string state { get; set; } = "";
    public int totalFilesToCopy { get; set; }
    public long totalFilesSize { get; set; }
    public int nbFilesLeftToDo { get; set; }
    public int progression { get; set; }
}