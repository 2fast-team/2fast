using OtpNet;
using Project2FA.Repository.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Project2FA.UWP.ViewModels
{
    public class TwoFACodeTestPageViewModel
    {
        DispatcherTimer _dispatcherTimer;
        public TwoFACodeTestPageViewModel()
        {
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1); //every second
            _dispatcherTimer.Tick += OnTimedEvent;
            Collection.Add(new TwoFACodeModel("TestA", "Blub_A", "ABCDEFGHIJLKMNO"));
            Collection.Add(new TwoFACodeModel("TestB", "Blub_B", "ABCDEFGHIJLKMNO"));
            Collection.Add(new TwoFACodeModel("TestC", "Blub_C", "ABCDEFGHIJLKMNO"));

            for (int i = 0; i < Collection.Count; i++)
            {
                var secretByteArray = Base32Encoding.ToBytes(Collection[i].SecretKey);
                var totp = new Totp(secretByteArray, step: Collection[i].SecondsToCount);
                Collection[i].TwoFACode = totp.ComputeTotp(DateTime.UtcNow);
                
            }
            _dispatcherTimer.Start();
        }

        private void OnTimedEvent(object sender, object e)
        {
            for (int i = 0; i < Collection.Count; i++)
            {
                if (Collection[i].Seconds == 0)
                {
                    _dispatcherTimer.Stop();
                    Collection[i].Seconds = Collection[i].SecondsToCount;
                    var secretByteArray = Base32Encoding.ToBytes(Collection[i].SecretKey);
                    var totp = new Totp(secretByteArray, step: Collection[i].SecondsToCount);
                    Collection[i].TwoFACode = totp.ComputeTotp(DateTime.UtcNow);
                    _dispatcherTimer.Start();
                }
                else
                {
                    Collection[i].Seconds--;
                }
            }
        }

        //private void OnTimedEvent(object sender, ElapsedEventArgs e)
        //{
        //    //TODO https://docs.microsoft.com/en-us/uwp/api/Windows.UI.Xaml.DispatcherTimer
        //    App.ShellPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        //    {
        //        for (int i = 0; i < Collection.Count; i++)
        //        {
        //            if (Collection[i].Seconds == 0)
        //            {
        //                _dispatcherTimer.Stop();
        //                Collection[i].Seconds = Collection[i].SecondsToCount;
        //                var secretByteArray = Base32Encoding.ToBytes(Collection[i].SecretKey);
        //                var totp = new Totp(secretByteArray, step: Collection[i].SecondsToCount);
        //                Collection[i].TwoFACode = totp.ComputeTotp(DateTime.UtcNow);
        //                _dispatcherTimer.Start();
        //            }
        //            else
        //            {
        //                Collection[i].Seconds--;
        //            }

        //        }
        //    });

        //}

        ObservableCollection<TwoFACodeModel> _collection;
        public ObservableCollection<TwoFACodeModel> Collection
        {
            get
            {
                if (_collection == null)
                {
                    _collection = new ObservableCollection<TwoFACodeModel>();
                }
                return _collection;
            }
            set => _collection = value;
        }
    }
}
