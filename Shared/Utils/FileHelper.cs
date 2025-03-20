namespace Shared.Utils
{
    public static class FileHelper
    {
        public static string GetFileFilter(TransferType type)
        {
            return type switch
            {
                TransferType.Image => "Ảnh (*.jpg;*.jpeg;*.png;*.gif)|*.jpg;*.jpeg;*.png;*.gif",
                TransferType.Video => "Video (*.mp4;*.avi;*.mov;*.mkv)|*.mp4;*.avi;*.mov;*.mkv",
                TransferType.Audio => "Âm thanh (*.mp3;*.wav;*.ogg;*.flac)|*.mp3;*.wav;*.ogg;*.flac",
                TransferType.File => "All Files (*.*)|*.*",
                TransferType.Folder => "Thư mục|.",
                _ => "All Files (*.*)|*.*"
            };
        }

        public static string GenerateFileName(TransferType type)
        {
            string prefix = type switch
            {
                TransferType.Image => "image",
                TransferType.Video => "video",
                TransferType.Audio => "audio",
                TransferType.Folder => "folder",
                _ => "file"
            };
            return $"{prefix}_{DateTime.Now:yyyyMMdd_HHmmss}";
        }

        public static TransferType GetTransferType(string path)
        {
            if (Directory.Exists(path)) return TransferType.Folder;
            
            string extension = Path.GetExtension(path).ToLower();
            
            return extension switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" => TransferType.Image,
                ".mp4" or ".avi" or ".mov" or ".mkv" or ".flv" or ".wmv" => TransferType.Video,
                ".mp3" or ".wav" or ".ogg" or ".flac" or ".aac" or ".m4a" => TransferType.Audio,
                _ => TransferType.File
            };
        }

        public static string GetFileTypeText(TransferType type)
        {
            return type switch
            {
                TransferType.Image => "📷 Ảnh",
                TransferType.Video => "🎥 Video",
                TransferType.Audio => "🔊 Âm thanh",
                TransferType.File => "📁 File",
                TransferType.Folder => "📂 Thư mục",
                TransferType.Text => "📝 Văn bản",
                _ => "📄 Tài liệu"
            };
        }
    }

    public enum TransferType
    {
        Text,
        Image,
        Video,
        Audio,
        File,
        Folder
    }
}