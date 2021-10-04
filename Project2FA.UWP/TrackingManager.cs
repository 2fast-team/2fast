// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.Services.Store.Engagement;
using Prism.Logging;

namespace Project2FA.UWP
{
    //https://github.com/windows-toolkit/WindowsCommunityToolkit/blob/master/Microsoft.Toolkit.Uwp.SampleApp/TrackingManager.cs
    public static class TrackingManager
    {
        private static readonly StoreServicesCustomEventLogger _logger;

        static TrackingManager()
        {
            try
            {
                _logger = StoreServicesCustomEventLogger.GetDefault();
            }
            catch
            {
                // Ignoring error
            }
        }

        public static void TrackException(Exception ex)
        {
            try
            {
                _logger.Log($"exception - {ex.Message} - {ex.StackTrace}");
            }
            catch
            {
                // Ignore error
            }
        }

        public static void TrackEvent(Category category, Priority priority, string label = "", long value = 0)
        {
            try
            {
                _logger.Log($"{category} - {priority} - {label} - {value.ToString()}");
            }
            catch
            {
                // Ignore error
            }
        }

        public static void TrackPage(string pageName)
        {
            try
            {
                _logger.Log($"pageView - {pageName}");
            }
            catch
            {
                // Ignore error
            }
        }
    }
}
