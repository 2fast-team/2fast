// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
#endif

namespace Project2FA.UWP.Controls
{
    /// <summary>
    /// Arguments relating to a change of Editor
    /// </summary>
    public class EditorChangedArgs
    {
        internal EditorChangedArgs()
        {
        }

        /// <summary>
        /// Gets the old Instance that is being Replaced
        /// </summary>
        public RichEditBox Old { get; internal set; }

        /// <summary>
        /// Gets the new Instance that is being Set
        /// </summary>
        public RichEditBox New { get; internal set; }
    }
}