using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.UI.Notifications;

namespace PixelRuler.Common
{
    public static class ToastHelper
    {
        public static void ConfigureToast(ToastNotification toast, bool requireUiThread, Action<ToastArguments> action)
        {
            void Toast_Activated(Windows.UI.Notifications.ToastNotification sender, object args)
            {
                toast.Activated -= Toast_Activated;

                var toastArgs = ((ToastActivatedEventArgs)args);
                ToastArguments toastArgsParsed = ToastArguments.Parse(toastArgs.Arguments);
                if(requireUiThread)
                {
                    Application.Current.Dispatcher.Invoke(() => action(toastArgsParsed));
                }
                else
                {
                    action(toastArgsParsed);
                }
            }
            toast.Activated += Toast_Activated;
        }

    }
}
