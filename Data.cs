using Charbacca.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace Charbacca
{
    public static class Data
    {
        static Task initTask;

        public static IList<Font> Fonts { get; private set; }

        public static UnicodeData UnicodeData { get; private set; }

        public static Task InitAsync()
        {
            if (initTask == null)
            {
                initTask = _InitAsync();
            }
            return initTask;
        }

        static async Task _InitAsync()
        {
            UnicodeData = await Task.Run(async () =>
            {
                var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Data/ucd.all.optimized.xml"));
                var ras = await file.OpenReadAsync();
                using (var stream = ras.AsStreamForRead())
                {
                    return UnicodeData.Load(stream);
                }
            });

            Fonts = (await FontData.LoadAsync()).OrderBy(f => f.Name).ToList();

#if DEBUG
            //var outputfolder = ApplicationData.Current.LocalFolder;
            //var outputfile = await outputfolder.CreateFileAsync("ucd.all.optimized.xml", CreationCollisionOption.ReplaceExisting);
            //var rasoutput = await outputfile.OpenAsync(FileAccessMode.ReadWrite);
            //await Task.Run(() =>
            //{
            //    using (var stream = rasoutput.AsStreamForWrite())
            //    {
            //        UnicodeData.Save(stream, UnicodeData);
            //    }
            //});
#endif
        }
    }
}
