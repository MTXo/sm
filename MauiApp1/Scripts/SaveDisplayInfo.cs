using MauiApp1.Scripts;

namespace MauiApp1.Scripts
{
    public class SaveDisplayInfo
    {
        public SaveInfo Save { get; set; } = null!;

        public string BranchName { get; set; } = string.Empty;

        public int Id => Save.Id;
        public string FileName => Save.FileName;
        public DateTime SaveTime => Save.SaveTime;
        public int BranchId => Save.BranchId;
    }
}
