// File generated automatically by ReswPlus. https://github.com/DotNetPlus/ReswPlus
// The NuGet package ReswPlusLib is necessary to support Pluralization.
using System;
using Windows.ApplicationModel.Resources;
#if WINDOWS_UWP
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Data;
#else
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Data;
#endif

namespace Project2FA.Strings
{
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("DotNetPlus.ReswPlus", "2.1.3")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public static class Resources
    {
        private static ResourceLoader _resourceLoader;
        static Resources()
        {
            _resourceLoader = ResourceLoader.GetForViewIndependentUse("Resources");
        }

        #region AccountCodePageCopied
        /// <summary>
        ///   Looks up a localized string similar to: Copied to clipboard
        /// </summary>
        public static string AccountCodePageCopied
        {
            get
            {
                return _resourceLoader.GetString("AccountCodePageCopied");
            }
        }
        #endregion

        #region AccountCodePageItemMoreBTNToolTip
        /// <summary>
        ///   Looks up a localized string similar to: Show options
        /// </summary>
        public static string AccountCodePageItemMoreBTNToolTip
        {
            get
            {
                return _resourceLoader.GetString("AccountCodePageItemMoreBTNToolTip");
            }
        }
        #endregion

        #region AccountCodePageViewModelDeleteAccept
        /// <summary>
        ///   Looks up a localized string similar to: Yes
        /// </summary>
        public static string AccountCodePageViewModelDeleteAccept
        {
            get
            {
                return _resourceLoader.GetString("AccountCodePageViewModelDeleteAccept");
            }
        }
        #endregion

        #region AccountCodePageViewModelDeleteCancel
        /// <summary>
        ///   Looks up a localized string similar to: No
        /// </summary>
        public static string AccountCodePageViewModelDeleteCancel
        {
            get
            {
                return _resourceLoader.GetString("AccountCodePageViewModelDeleteCancel");
            }
        }
        #endregion

        #region DeleteDatafileContentDialogDeleteDescription
        /// <summary>
        ///   Looks up a localized string similar to: Are you sure that you want to delete this account irrevocably?
        /// </summary>
        public static string DeleteDatafileContentDialogDeleteDescription
        {
            get
            {
                return _resourceLoader.GetString("DeleteDatafileContentDialogDeleteDescription");
            }
        }
        #endregion

        #region DeleteDatafileContentDialogTitle
        /// <summary>
        ///   Looks up a localized string similar to: Confirm deletion
        /// </summary>
        public static string DeleteDatafileContentDialogTitle
        {
            get
            {
                return _resourceLoader.GetString("DeleteDatafileContentDialogTitle");
            }
        }
        #endregion

        #region AccountCopyCodeButtonToolTip
        /// <summary>
        ///   Looks up a localized string similar to: copy code
        /// </summary>
        public static string AccountCopyCodeButtonToolTip
        {
            get
            {
                return _resourceLoader.GetString("AccountCopyCodeButtonToolTip");
            }
        }
        #endregion

        #region AddAccountContentDialogQRCodeContentError
        /// <summary>
        ///   Looks up a localized string similar to: Can't get information from QR code. Please try again or add the secret key manually.
        /// </summary>
        public static string AddAccountContentDialogQRCodeContentError
        {
            get
            {
                return _resourceLoader.GetString("AddAccountContentDialogQRCodeContentError");
            }
        }
        #endregion

        #region AddAccountContentDialogReadQRCodeTeachingTipInfo
        /// <summary>
        ///   Looks up a localized string similar to: A scan tool will open soon
        /// </summary>
        public static string AddAccountContentDialogReadQRCodeTeachingTipInfo
        {
            get
            {
                return _resourceLoader.GetString("AddAccountContentDialogReadQRCodeTeachingTipInfo");
            }
        }
        #endregion

        #region Confirm
        /// <summary>
        ///   Looks up a localized string similar to: Confirm
        /// </summary>
        public static string Confirm
        {
            get
            {
                return _resourceLoader.GetString("Confirm");
            }
        }
        #endregion

        #region Delete
        /// <summary>
        ///   Looks up a localized string similar to: delete
        /// </summary>
        public static string Delete
        {
            get
            {
                return _resourceLoader.GetString("Delete");
            }
        }
        #endregion

        #region Edit
        /// <summary>
        ///   Looks up a localized string similar to: edit
        /// </summary>
        public static string Edit
        {
            get
            {
                return _resourceLoader.GetString("Edit");
            }
        }
        #endregion

        #region Error
        /// <summary>
        ///   Looks up a localized string similar to: Error
        /// </summary>
        public static string Error
        {
            get
            {
                return _resourceLoader.GetString("Error");
            }
        }
        #endregion

        #region LoginPagePasswordMismatch
        /// <summary>
        ///   Looks up a localized string similar to: The password doesn't match the password of the data file.
        /// </summary>
        public static string LoginPagePasswordMismatch
        {
            get
            {
                return _resourceLoader.GetString("LoginPagePasswordMismatch");
            }
        }
        #endregion

        #region No
        /// <summary>
        ///   Looks up a localized string similar to: No
        /// </summary>
        public static string No
        {
            get
            {
                return _resourceLoader.GetString("No");
            }
        }
        #endregion

        #region SettingsActionRequiredRestart
        /// <summary>
        ///   Looks up a localized string similar to: This change will take effect after restart of the app.
        /// </summary>
        public static string SettingsActionRequiredRestart
        {
            get
            {
                return _resourceLoader.GetString("SettingsActionRequiredRestart");
            }
        }
        #endregion

        #region SettingsFactoryResetMessage
        /// <summary>
        ///   Looks up a localized string similar to: Are you sure you want to reset the app to factory settings? **This process cannot be undone!** The datafile will **not** be removed in the reset progress.
        /// </summary>
        public static string SettingsFactoryResetMessage
        {
            get
            {
                return _resourceLoader.GetString("SettingsFactoryResetMessage");
            }
        }
        #endregion

        #region SettingsGiveFeedbackText
        /// <summary>
        ///   Looks up a localized string similar to: give feedback
        /// </summary>
        public static string SettingsGiveFeedbackText
        {
            get
            {
                return _resourceLoader.GetString("SettingsGiveFeedbackText");
            }
        }
        #endregion

        #region SettingsRemoveDatafileMessage
        /// <summary>
        ///   Looks up a localized string similar to: Are you sure you want to permanently delete the data file? You will loose all your added accounts.
        /// </summary>
        public static string SettingsRemoveDatafileMessage
        {
            get
            {
                return _resourceLoader.GetString("SettingsRemoveDatafileMessage");
            }
        }
        #endregion

        #region ShellPageFeedbackNavigationToolTip
        /// <summary>
        ///   Looks up a localized string similar to: give feedback
        /// </summary>
        public static string ShellPageFeedbackNavigationToolTip
        {
            get
            {
                return _resourceLoader.GetString("ShellPageFeedbackNavigationToolTip");
            }
        }
        #endregion

        #region WindowsHelloLoginMessage
        /// <summary>
        ///   Looks up a localized string similar to: Please verify now with your configured Windows Hello login.
        /// </summary>
        public static string WindowsHelloLoginMessage
        {
            get
            {
                return _resourceLoader.GetString("WindowsHelloLoginMessage");
            }
        }
        #endregion

        #region WindowsHelloPreferMessage
        /// <summary>
        ///   Looks up a localized string similar to: It is possible to log in with Windows Hello. Would you like to prefer this login in the future? You can change this option in the settings at any time.
        /// </summary>
        public static string WindowsHelloPreferMessage
        {
            get
            {
                return _resourceLoader.GetString("WindowsHelloPreferMessage");
            }
        }
        #endregion

        #region Yes
        /// <summary>
        ///   Looks up a localized string similar to: Yes
        /// </summary>
        public static string Yes
        {
            get
            {
                return _resourceLoader.GetString("Yes");
            }
        }
        #endregion

        #region AccountCodePageRemainingSecondsToolTip
        /// <summary>
        ///   Looks up a localized string similar to: Seconds to refresh
        /// </summary>
        public static string AccountCodePageRemainingSecondsToolTip
        {
            get
            {
                return _resourceLoader.GetString("AccountCodePageRemainingSecondsToolTip");
            }
        }
        #endregion

        #region ButtonTextCancel
        /// <summary>
        ///   Looks up a localized string similar to: Cancel
        /// </summary>
        public static string ButtonTextCancel
        {
            get
            {
                return _resourceLoader.GetString("ButtonTextCancel");
            }
        }
        #endregion

        #region ButtonTextConfirm
        /// <summary>
        ///   Looks up a localized string similar to: Confirm
        /// </summary>
        public static string ButtonTextConfirm
        {
            get
            {
                return _resourceLoader.GetString("ButtonTextConfirm");
            }
        }
        #endregion

        #region ButtonTextNo
        /// <summary>
        ///   Looks up a localized string similar to: No
        /// </summary>
        public static string ButtonTextNo
        {
            get
            {
                return _resourceLoader.GetString("ButtonTextNo");
            }
        }
        #endregion

        #region ButtonTextYes
        /// <summary>
        ///   Looks up a localized string similar to: Yes
        /// </summary>
        public static string ButtonTextYes
        {
            get
            {
                return _resourceLoader.GetString("ButtonTextYes");
            }
        }
        #endregion

        #region RateAppContentDialogLaterButton
        /// <summary>
        ///   Looks up a localized string similar to: recall later
        /// </summary>
        public static string RateAppContentDialogLaterButton
        {
            get
            {
                return _resourceLoader.GetString("RateAppContentDialogLaterButton");
            }
        }
        #endregion

        #region RateAppContentDialogNoButton
        /// <summary>
        ///   Looks up a localized string similar to: no, thanks
        /// </summary>
        public static string RateAppContentDialogNoButton
        {
            get
            {
                return _resourceLoader.GetString("RateAppContentDialogNoButton");
            }
        }
        #endregion

        #region RateAppContentDialogQuestion
        /// <summary>
        ///   Looks up a localized string similar to: You have been using this app for some time now, please visit the store to rate it and write a review. You can also use the Windows Feedback App at any time to send us your feedback directly.
        /// </summary>
        public static string RateAppContentDialogQuestion
        {
            get
            {
                return _resourceLoader.GetString("RateAppContentDialogQuestion");
            }
        }
        #endregion

        #region RateAppContentDialogYesButton
        /// <summary>
        ///   Looks up a localized string similar to: Rate now
        /// </summary>
        public static string RateAppContentDialogYesButton
        {
            get
            {
                return _resourceLoader.GetString("RateAppContentDialogYesButton");
            }
        }
        #endregion

        #region RateAppContentDialogTitle
        /// <summary>
        ///   Looks up a localized string similar to: Do you like this app?
        /// </summary>
        public static string RateAppContentDialogTitle
        {
            get
            {
                return _resourceLoader.GetString("RateAppContentDialogTitle");
            }
        }
        #endregion

        #region AuthorizationFileSystemContentDialogDescription
        /// <summary>
        ///   Looks up a localized string similar to: The application does not currently have the necessary access rights to access the data file. To set the rights, however, the application must be closed. Please grant the required rights in the settings and restart the application. Instructions for setting the authorisation: ![Instructions for setting the authorisation](ms-appx:///Assets/Infoelements/AuthSystemstorageEN.png)
        /// </summary>
        public static string AuthorizationFileSystemContentDialogDescription
        {
            get
            {
                return _resourceLoader.GetString("AuthorizationFileSystemContentDialogDescription");
            }
        }
        #endregion

        #region AuthorizationFileSystemContentDialogPrimaryBTN
        /// <summary>
        ///   Looks up a localized string similar to: Set authorization
        /// </summary>
        public static string AuthorizationFileSystemContentDialogPrimaryBTN
        {
            get
            {
                return _resourceLoader.GetString("AuthorizationFileSystemContentDialogPrimaryBTN");
            }
        }
        #endregion

        #region AuthorizationFileSystemContentDialogSecondaryBTN
        /// <summary>
        ///   Looks up a localized string similar to: Cancel (close application)
        /// </summary>
        public static string AuthorizationFileSystemContentDialogSecondaryBTN
        {
            get
            {
                return _resourceLoader.GetString("AuthorizationFileSystemContentDialogSecondaryBTN");
            }
        }
        #endregion

        #region AuthorizationFileSystemContentDialogTitle
        /// <summary>
        ///   Looks up a localized string similar to: Authorization error
        /// </summary>
        public static string AuthorizationFileSystemContentDialogTitle
        {
            get
            {
                return _resourceLoader.GetString("AuthorizationFileSystemContentDialogTitle");
            }
        }
        #endregion

        #region AccountCodePageCopyCodeTeachingTip
        /// <summary>
        ///   Looks up a localized string similar to: Key copied to clipboard
        /// </summary>
        public static string AccountCodePageCopyCodeTeachingTip
        {
            get
            {
                return _resourceLoader.GetString("AccountCodePageCopyCodeTeachingTip");
            }
        }
        #endregion

        #region ChangePassword
        /// <summary>
        ///   Looks up a localized string similar to: Change password
        /// </summary>
        public static string ChangePassword
        {
            get
            {
                return _resourceLoader.GetString("ChangePassword");
            }
        }
        #endregion

        #region CloseApp
        /// <summary>
        ///   Looks up a localized string similar to: Close app
        /// </summary>
        public static string CloseApp
        {
            get
            {
                return _resourceLoader.GetString("CloseApp");
            }
        }
        #endregion

        #region DeleteAccountContentDialogDescription
        /// <summary>
        ///   Looks up a localized string similar to: Are you sure you want to delete the account? After the deletion you have **30 seconds** to undo the deletion.
        /// </summary>
        public static string DeleteAccountContentDialogDescription
        {
            get
            {
                return _resourceLoader.GetString("DeleteAccountContentDialogDescription");
            }
        }
        #endregion

        #region DeleteAccountContentDialogTitle
        /// <summary>
        ///   Looks up a localized string similar to: Delete account
        /// </summary>
        public static string DeleteAccountContentDialogTitle
        {
            get
            {
                return _resourceLoader.GetString("DeleteAccountContentDialogTitle");
            }
        }
        #endregion

        #region ErrorHandle
        /// <summary>
        ///   Looks up a localized string similar to: Error handle
        /// </summary>
        public static string ErrorHandle
        {
            get
            {
                return _resourceLoader.GetString("ErrorHandle");
            }
        }
        #endregion

        #region ErrorHandleDescription
        /// <summary>
        ///   Looks up a localized string similar to: An unhandled error has occurred, please send the error details which can be seen in the field via the feedback hub, or add a new issue via Github.
        /// </summary>
        public static string ErrorHandleDescription
        {
            get
            {
                return _resourceLoader.GetString("ErrorHandleDescription");
            }
        }
        #endregion

        #region PasswordInvalidHeader
        /// <summary>
        ///   Looks up a localized string similar to: Password invalid
        /// </summary>
        public static string PasswordInvalidHeader
        {
            get
            {
                return _resourceLoader.GetString("PasswordInvalidHeader");
            }
        }
        #endregion

        #region SettingsQRScanningHeader
        /// <summary>
        ///   Looks up a localized string similar to: Time delay to start reading the QR Code
        /// </summary>
        public static string SettingsQRScanningHeader
        {
            get
            {
                return _resourceLoader.GetString("SettingsQRScanningHeader");
            }
        }
        #endregion

        #region SettingsQRScanningSecondsRange
        /// <summary>
        ///   Looks up a localized string similar to: Specified in seconds (3-10s allowed)
        /// </summary>
        public static string SettingsQRScanningSecondsRange
        {
            get
            {
                return _resourceLoader.GetString("SettingsQRScanningSecondsRange");
            }
        }
        #endregion

        #region ExceptionDatafileNotFound
        /// <summary>
        ///   Looks up a localized string similar to: The path to the current data file could not be found. Possibly the data file was moved by you. **Do you want to edit the path?**
        /// </summary>
        public static string ExceptionDatafileNotFound
        {
            get
            {
                return _resourceLoader.GetString("ExceptionDatafileNotFound");
            }
        }
        #endregion

        #region AccountCodePageABBLogoutToolTip
        /// <summary>
        ///   Looks up a localized string similar to: lock app
        /// </summary>
        public static string AccountCodePageABBLogoutToolTip
        {
            get
            {
                return _resourceLoader.GetString("AccountCodePageABBLogoutToolTip");
            }
        }
        #endregion

        #region AccountCodePageAddEntryToolTip
        /// <summary>
        ///   Looks up a localized string similar to: Add account
        /// </summary>
        public static string AccountCodePageAddEntryToolTip
        {
            get
            {
                return _resourceLoader.GetString("AccountCodePageAddEntryToolTip");
            }
        }
        #endregion

        #region AccountCodePageReloadDatafileToolTip
        /// <summary>
        ///   Looks up a localized string similar to: Reload datafile
        /// </summary>
        public static string AccountCodePageReloadDatafileToolTip
        {
            get
            {
                return _resourceLoader.GetString("AccountCodePageReloadDatafileToolTip");
            }
        }
        #endregion

        #region PasswordInvalidMessage
        /// <summary>
        ///   Looks up a localized string similar to: The password saved in this app is invalid for the data file. You may have changed the password in another machine for the data file. **Do you want to change the password now?
        /// </summary>
        public static string PasswordInvalidMessage
        {
            get
            {
                return _resourceLoader.GetString("PasswordInvalidMessage");
            }
        }
        #endregion

        #region SettingsFactoryResetDialogTitle
        /// <summary>
        ///   Looks up a localized string similar to: Factory reset
        /// </summary>
        public static string SettingsFactoryResetDialogTitle
        {
            get
            {
                return _resourceLoader.GetString("SettingsFactoryResetDialogTitle");
            }
        }
        #endregion

        #region AccountCodePageABBUndoDeleteToolTip
        /// <summary>
        ///   Looks up a localized string similar to: Undo delete
        /// </summary>
        public static string AccountCodePageABBUndoDeleteToolTip
        {
            get
            {
                return _resourceLoader.GetString("AccountCodePageABBUndoDeleteToolTip");
            }
        }
        #endregion

        #region NewDatafileContentDialogDatafileExistsError
        /// <summary>
        ///   Looks up a localized string similar to: A data file with this name already exists in the folder. Please change the folder or the name of the file.
        /// </summary>
        public static string NewDatafileContentDialogDatafileExistsError
        {
            get
            {
                return _resourceLoader.GetString("NewDatafileContentDialogDatafileExistsError");
            }
        }
        #endregion

        #region ErrorHandleSendError
        /// <summary>
        ///   Looks up a localized string similar to: send error
        /// </summary>
        public static string ErrorHandleSendError
        {
            get
            {
                return _resourceLoader.GetString("ErrorHandleSendError");
            }
        }
        #endregion

        #region FactoryReset
        /// <summary>
        ///   Looks up a localized string similar to: factory reset
        /// </summary>
        public static string FactoryReset
        {
            get
            {
                return _resourceLoader.GetString("FactoryReset");
            }
        }
        #endregion

        #region ChangeDatafilePath
        /// <summary>
        ///   Looks up a localized string similar to: Change file path
        /// </summary>
        public static string ChangeDatafilePath
        {
            get
            {
                return _resourceLoader.GetString("ChangeDatafilePath");
            }
        }
        #endregion

        #region AccountCodePageWrongTimeBTN
        /// <summary>
        ///   Looks up a localized string similar to: Open time settings
        /// </summary>
        public static string AccountCodePageWrongTimeBTN
        {
            get
            {
                return _resourceLoader.GetString("AccountCodePageWrongTimeBTN");
            }
        }
        #endregion

        #region AccountCodePageWrongTimeContent
        /// <summary>
        ///   Looks up a localized string similar to: The current time display of the device is incorrect, so the codes for the accounts cannot be generated correctly. Please synchronize the time in the settings.
        /// </summary>
        public static string AccountCodePageWrongTimeContent
        {
            get
            {
                return _resourceLoader.GetString("AccountCodePageWrongTimeContent");
            }
        }
        #endregion

        #region AccountCodePageWrongTimeTitle
        /// <summary>
        ///   Looks up a localized string similar to: Incorrect time display
        /// </summary>
        public static string AccountCodePageWrongTimeTitle
        {
            get
            {
                return _resourceLoader.GetString("AccountCodePageWrongTimeTitle");
            }
        }
        #endregion

        #region SettingsDatafileChangePasswordInfoTitle
        /// <summary>
        ///   Looks up a localized string similar to: Password has been changed successfully
        /// </summary>
        public static string SettingsDatafileChangePasswordInfoTitle
        {
            get
            {
                return _resourceLoader.GetString("SettingsDatafileChangePasswordInfoTitle");
            }
        }
        #endregion

        #region ShellPagePaneTitle
        /// <summary>
        ///   Looks up a localized string similar to: Navigation menu
        /// </summary>
        public static string ShellPagePaneTitle
        {
            get
            {
                return _resourceLoader.GetString("ShellPagePaneTitle");
            }
        }
        #endregion

        #region AccountCodePageAutoSuggestBoxPlaceholder
        /// <summary>
        ///   Looks up a localized string similar to: Search
        /// </summary>
        public static string AccountCodePageAutoSuggestBoxPlaceholder
        {
            get
            {
                return _resourceLoader.GetString("AccountCodePageAutoSuggestBoxPlaceholder");
            }
        }
        #endregion

        #region AccountCodePageSearchNotFound
        /// <summary>
        ///   Looks up a localized string similar to: No results found
        /// </summary>
        public static string AccountCodePageSearchNotFound
        {
            get
            {
                return _resourceLoader.GetString("AccountCodePageSearchNotFound");
            }
        }
        #endregion

        #region SettingsDependencyGroupAssets
        /// <summary>
        ///   Looks up a localized string similar to: Assets
        /// </summary>
        public static string SettingsDependencyGroupAssets
        {
            get
            {
                return _resourceLoader.GetString("SettingsDependencyGroupAssets");
            }
        }
        #endregion

        #region SettingsDependencyGroupPackages
        /// <summary>
        ///   Looks up a localized string similar to: Packages
        /// </summary>
        public static string SettingsDependencyGroupPackages
        {
            get
            {
                return _resourceLoader.GetString("SettingsDependencyGroupPackages");
            }
        }
        #endregion

        #region RestartApp
        /// <summary>
        ///   Looks up a localized string similar to: restart app
        /// </summary>
        public static string RestartApp
        {
            get
            {
                return _resourceLoader.GetString("RestartApp");
            }
        }
        #endregion

        #region ErrorCopyToClipboard
        /// <summary>
        ///   Looks up a localized string similar to: Copy report to clipboard
        /// </summary>
        public static string ErrorCopyToClipboard
        {
            get
            {
                return _resourceLoader.GetString("ErrorCopyToClipboard");
            }
        }
        #endregion

        #region ErrorHandleDescriptionLastSession
        /// <summary>
        ///   Looks up a localized string similar to: An unhandled error occurred the last time the app was used, please submit the error details which can be seen in the box via the feedback hub, or use the error message via Github.
        /// </summary>
        public static string ErrorHandleDescriptionLastSession
        {
            get
            {
                return _resourceLoader.GetString("ErrorHandleDescriptionLastSession");
            }
        }
        #endregion

        #region NewAppFeaturesContent
        /// <summary>
        ///   Looks up a localized string similar to: - Bug fixes - Updating third-party libraries
        /// </summary>
        public static string NewAppFeaturesContent
        {
            get
            {
                return _resourceLoader.GetString("NewAppFeaturesContent");
            }
        }
        #endregion

        #region NewAppFeaturesTitle
        /// <summary>
        ///   Looks up a localized string similar to: New features & changes
        /// </summary>
        public static string NewAppFeaturesTitle
        {
            get
            {
                return _resourceLoader.GetString("NewAppFeaturesTitle");
            }
        }
        #endregion

        #region WelcomePageTitle
        /// <summary>
        ///   Looks up a localized string similar to: Welcome
        /// </summary>
        public static string WelcomePageTitle
        {
            get
            {
                return _resourceLoader.GetString("WelcomePageTitle");
            }
        }
        #endregion

        #region CreateDatafile
        /// <summary>
        ///   Looks up a localized string similar to: Create datafile
        /// </summary>
        public static string CreateDatafile
        {
            get
            {
                return _resourceLoader.GetString("CreateDatafile");
            }
        }
        #endregion

        #region LoadDatafile
        /// <summary>
        ///   Looks up a localized string similar to: Load existing datafile
        /// </summary>
        public static string LoadDatafile
        {
            get
            {
                return _resourceLoader.GetString("LoadDatafile");
            }
        }
        #endregion

        #region UseDatafilePasswordInfo
        /// <summary>
        ///   Looks up a localized string similar to: To load the data file, you must enter the password that was configured when the file was created.
        /// </summary>
        public static string UseDatafilePasswordInfo
        {
            get
            {
                return _resourceLoader.GetString("UseDatafilePasswordInfo");
            }
        }
        #endregion

        #region ToolTipShowHelp
        /// <summary>
        ///   Looks up a localized string similar to: show help
        /// </summary>
        public static string ToolTipShowHelp
        {
            get
            {
                return _resourceLoader.GetString("ToolTipShowHelp");
            }
        }
        #endregion

        #region SettingsAutoLogoutDesc
        /// <summary>
        ///   Looks up a localized string similar to: Automatic logout after inactivity
        /// </summary>
        public static string SettingsAutoLogoutDesc
        {
            get
            {
                return _resourceLoader.GetString("SettingsAutoLogoutDesc");
            }
        }
        #endregion

        #region SettingsAutoLogoutMinutes
        /// <summary>
        ///   Looks up a localized string similar to: minutes inactivity
        /// </summary>
        public static string SettingsAutoLogoutMinutes
        {
            get
            {
                return _resourceLoader.GetString("SettingsAutoLogoutMinutes");
            }
        }
        #endregion

        #region Close
        /// <summary>
        ///   Looks up a localized string similar to: Close
        /// </summary>
        public static string Close
        {
            get
            {
                return _resourceLoader.GetString("Close");
            }
        }
        #endregion

        #region AddAccountCodeContentDialogQRCodeHelp
        /// <summary>
        ///   Looks up a localized string similar to: If you select the QR-code scan, a reading tool will open shortly after pushing the button. Change the window to the service and make a rectangle around the QR-code by holding down your left mouse key.
        /// </summary>
        public static string AddAccountCodeContentDialogQRCodeHelp
        {
            get
            {
                return _resourceLoader.GetString("AddAccountCodeContentDialogQRCodeHelp");
            }
        }
        #endregion

        #region DisplayQRCodeContentDialogTitle
        /// <summary>
        ///   Looks up a localized string similar to: QR code display
        /// </summary>
        public static string DisplayQRCodeContentDialogTitle
        {
            get
            {
                return _resourceLoader.GetString("DisplayQRCodeContentDialogTitle");
            }
        }
        #endregion

        #region AddAccountCodeContentDialogInputSecretKeyHelp
        /// <summary>
        ///   Looks up a localized string similar to: The input is based on Base32 and therefore only allows the letters A-Z and the numbers from 2-7.
        /// </summary>
        public static string AddAccountCodeContentDialogInputSecretKeyHelp
        {
            get
            {
                return _resourceLoader.GetString("AddAccountCodeContentDialogInputSecretKeyHelp");
            }
        }
        #endregion

        #region AccountCodePageTooltipDeleteFavourite
        /// <summary>
        ///   Looks up a localized string similar to: Undo favourite
        /// </summary>
        public static string AccountCodePageTooltipDeleteFavourite
        {
            get
            {
                return _resourceLoader.GetString("AccountCodePageTooltipDeleteFavourite");
            }
        }
        #endregion

        #region AccountCodePageTooltipSetFavourite
        /// <summary>
        ///   Looks up a localized string similar to: Set as favourite
        /// </summary>
        public static string AccountCodePageTooltipSetFavourite
        {
            get
            {
                return _resourceLoader.GetString("AccountCodePageTooltipSetFavourite");
            }
        }
        #endregion

        #region AccountCodePageTooltipHideTOTP
        /// <summary>
        ///   Looks up a localized string similar to: hide key
        /// </summary>
        public static string AccountCodePageTooltipHideTOTP
        {
            get
            {
                return _resourceLoader.GetString("AccountCodePageTooltipHideTOTP");
            }
        }
        #endregion

        #region AccountCodePageTooltipShowTOTP
        /// <summary>
        ///   Looks up a localized string similar to: show key
        /// </summary>
        public static string AccountCodePageTooltipShowTOTP
        {
            get
            {
                return _resourceLoader.GetString("AccountCodePageTooltipShowTOTP");
            }
        }
        #endregion

        #region ButtonTextRetry
        /// <summary>
        ///   Looks up a localized string similar to: Try again
        /// </summary>
        public static string ButtonTextRetry
        {
            get
            {
                return _resourceLoader.GetString("ButtonTextRetry");
            }
        }
        #endregion

        #region ErrorClipboardTask
        /// <summary>
        ///   Looks up a localized string similar to: The clipboard has been occupied by another program, please try again.
        /// </summary>
        public static string ErrorClipboardTask
        {
            get
            {
                return _resourceLoader.GetString("ErrorClipboardTask");
            }
        }
        #endregion

        #region AddAccountCodeContentDialogExpertSettingsHelp
        /// <summary>
        ///   Looks up a localized string similar to: Only change these settings if you are sure that the service requires them.
        /// </summary>
        public static string AddAccountCodeContentDialogExpertSettingsHelp
        {
            get
            {
                return _resourceLoader.GetString("AddAccountCodeContentDialogExpertSettingsHelp");
            }
        }
        #endregion

        #region AccountCodePageSearchFilterToolTip
        /// <summary>
        ///   Looks up a localized string similar to: Filter
        /// </summary>
        public static string AccountCodePageSearchFilterToolTip
        {
            get
            {
                return _resourceLoader.GetString("AccountCodePageSearchFilterToolTip");
            }
        }
        #endregion

        #region WebViewDatafileContentDialogChooseFile
        /// <summary>
        ///   Looks up a localized string similar to: Choose file
        /// </summary>
        public static string WebViewDatafileContentDialogChooseFile
        {
            get
            {
                return _resourceLoader.GetString("WebViewDatafileContentDialogChooseFile");
            }
        }
        #endregion

        #region WebViewDatafileContentDialogChooseFolder
        /// <summary>
        ///   Looks up a localized string similar to: Choose folder
        /// </summary>
        public static string WebViewDatafileContentDialogChooseFolder
        {
            get
            {
                return _resourceLoader.GetString("WebViewDatafileContentDialogChooseFolder");
            }
        }
        #endregion

        #region WelcomePageTutorialTipOpen
        /// <summary>
        ///   Looks up a localized string similar to: Open tutorial
        /// </summary>
        public static string WelcomePageTutorialTipOpen
        {
            get
            {
                return _resourceLoader.GetString("WelcomePageTutorialTipOpen");
            }
        }
        #endregion

        #region WelcomePageTutorialDesc
        /// <summary>
        ///   Looks up a localized string similar to: Would you like to open the tutorial for the application?
        /// </summary>
        public static string WelcomePageTutorialDesc
        {
            get
            {
                return _resourceLoader.GetString("WelcomePageTutorialDesc");
            }
        }
        #endregion

        #region FunctionNotAvailable
        /// <summary>
        ///   Looks up a localized string similar to: Function not available
        /// </summary>
        public static string FunctionNotAvailable
        {
            get
            {
                return _resourceLoader.GetString("FunctionNotAvailable");
            }
        }
        #endregion

        #region WindowsHelloNotAvailable
        /// <summary>
        ///   Looks up a localized string similar to: Windows Hello cannot be executed because no password is stored for the datafile in the protected memory.
        /// </summary>
        public static string WindowsHelloNotAvailable
        {
            get
            {
                return _resourceLoader.GetString("WindowsHelloNotAvailable");
            }
        }
        #endregion

        #region ErrorGenerateTOTPCode
        /// <summary>
        ///   Looks up a localized string similar to: The secret key for the account {0} is empty or unreadable. Please restart the app. If the problem persists, clean up the account {0} and add it again.
        /// </summary>
        public static string ErrorGenerateTOTPCode
        {
            get
            {
                return _resourceLoader.GetString("ErrorGenerateTOTPCode");
            }
        }
        #endregion

        #region TextFormatterBoldToolTip
        /// <summary>
        ///   Looks up a localized string similar to: Bold
        /// </summary>
        public static string TextFormatterBoldToolTip
        {
            get
            {
                return _resourceLoader.GetString("TextFormatterBoldToolTip");
            }
        }
        #endregion

        #region TextFormatterItalicToolTip
        /// <summary>
        ///   Looks up a localized string similar to: Italics
        /// </summary>
        public static string TextFormatterItalicToolTip
        {
            get
            {
                return _resourceLoader.GetString("TextFormatterItalicToolTip");
            }
        }
        #endregion

        #region TextFormatterStrikeToolTip
        /// <summary>
        ///   Looks up a localized string similar to: Strikethrough
        /// </summary>
        public static string TextFormatterStrikeToolTip
        {
            get
            {
                return _resourceLoader.GetString("TextFormatterStrikeToolTip");
            }
        }
        #endregion

        #region TextFormatterUnderlineToolTip
        /// <summary>
        ///   Looks up a localized string similar to: Underline
        /// </summary>
        public static string TextFormatterUnderlineToolTip
        {
            get
            {
                return _resourceLoader.GetString("TextFormatterUnderlineToolTip");
            }
        }
        #endregion

        #region TextFormatterListToolTip
        /// <summary>
        ///   Looks up a localized string similar to: List
        /// </summary>
        public static string TextFormatterListToolTip
        {
            get
            {
                return _resourceLoader.GetString("TextFormatterListToolTip");
            }
        }
        #endregion

        #region TextFormatterOrderedToolTip
        /// <summary>
        ///   Looks up a localized string similar to: Ordered list
        /// </summary>
        public static string TextFormatterOrderedToolTip
        {
            get
            {
                return _resourceLoader.GetString("TextFormatterOrderedToolTip");
            }
        }
        #endregion

        #region WriteDatafileErrorBTNCancel
        /// <summary>
        ///   Looks up a localized string similar to: Discard changes
        /// </summary>
        public static string WriteDatafileErrorBTNCancel
        {
            get
            {
                return _resourceLoader.GetString("WriteDatafileErrorBTNCancel");
            }
        }
        #endregion

        #region WriteDatafileErrorBTNRetry
        /// <summary>
        ///   Looks up a localized string similar to: Save again
        /// </summary>
        public static string WriteDatafileErrorBTNRetry
        {
            get
            {
                return _resourceLoader.GetString("WriteDatafileErrorBTNRetry");
            }
        }
        #endregion

        #region WriteDatafileErrorDesc
        /// <summary>
        ///   Looks up a localized string similar to: The changes could not be saved. Do you want to save them again?
        /// </summary>
        public static string WriteDatafileErrorDesc
        {
            get
            {
                return _resourceLoader.GetString("WriteDatafileErrorDesc");
            }
        }
        #endregion

        #region DatafileViewModelWebDAVServerNotFound
        /// <summary>
        ///   Looks up a localized string similar to: Address not found
        /// </summary>
        public static string DatafileViewModelWebDAVServerNotFound
        {
            get
            {
                return _resourceLoader.GetString("DatafileViewModelWebDAVServerNotFound");
            }
        }
        #endregion

        #region DatafileViewModelWebDAVServerNotFoundDesc
        /// <summary>
        ///   Looks up a localized string similar to: The address to the WebDAV service could not be found.
        /// </summary>
        public static string DatafileViewModelWebDAVServerNotFoundDesc
        {
            get
            {
                return _resourceLoader.GetString("DatafileViewModelWebDAVServerNotFoundDesc");
            }
        }
        #endregion

        #region WebDAVAppPasswordInfo
        /// <summary>
        ///   Looks up a localized string similar to: An app password is required to log in to the WebDAV service, this can be created in the settings of the WebDAV service.
        /// </summary>
        public static string WebDAVAppPasswordInfo
        {
            get
            {
                return _resourceLoader.GetString("WebDAVAppPasswordInfo");
            }
        }
        #endregion

        #region DatafileViewModelWebDAVCredentialsError
        /// <summary>
        ///   Looks up a localized string similar to: Login error
        /// </summary>
        public static string DatafileViewModelWebDAVCredentialsError
        {
            get
            {
                return _resourceLoader.GetString("DatafileViewModelWebDAVCredentialsError");
            }
        }
        #endregion

        #region DatafileViewModelWebDAVCredentialsErrorDesc
        /// <summary>
        ///   Looks up a localized string similar to: Your username or password is invalid.
        /// </summary>
        public static string DatafileViewModelWebDAVCredentialsErrorDesc
        {
            get
            {
                return _resourceLoader.GetString("DatafileViewModelWebDAVCredentialsErrorDesc");
            }
        }
        #endregion

        #region DatafileViewModelWebDAVMaintenanceError
        /// <summary>
        ///   Looks up a localized string similar to: Server in maintenance mode
        /// </summary>
        public static string DatafileViewModelWebDAVMaintenanceError
        {
            get
            {
                return _resourceLoader.GetString("DatafileViewModelWebDAVMaintenanceError");
            }
        }
        #endregion

        #region DatafileViewModelWebDAVMaintenanceErrorDesc
        /// <summary>
        ///   Looks up a localized string similar to: Login is currently not possible because the WebDAV service is currently in maintenance mode.
        /// </summary>
        public static string DatafileViewModelWebDAVMaintenanceErrorDesc
        {
            get
            {
                return _resourceLoader.GetString("DatafileViewModelWebDAVMaintenanceErrorDesc");
            }
        }
        #endregion

        #region DatafileViewModelWebDAVNotInstalledError
        /// <summary>
        ///   Looks up a localized string similar to: Server not installed
        /// </summary>
        public static string DatafileViewModelWebDAVNotInstalledError
        {
            get
            {
                return _resourceLoader.GetString("DatafileViewModelWebDAVNotInstalledError");
            }
        }
        #endregion

        #region DatafileViewModelWebDAVNotInstalledErrorDesc
        /// <summary>
        ///   Looks up a localized string similar to: The WebDAV service is currently not properly installed.
        /// </summary>
        public static string DatafileViewModelWebDAVNotInstalledErrorDesc
        {
            get
            {
                return _resourceLoader.GetString("DatafileViewModelWebDAVNotInstalledErrorDesc");
            }
        }
        #endregion

        #region InfoBarDatafileNoInternet
        /// <summary>
        ///   Looks up a localized string similar to: Currently no internet connectivity is available for synchronisation. Please connect and confirm "Reload datafile".
        /// </summary>
        public static string InfoBarDatafileNoInternet
        {
            get
            {
                return _resourceLoader.GetString("InfoBarDatafileNoInternet");
            }
        }
        #endregion

        #region InfoBarDatafileTitleNoInternet
        /// <summary>
        ///   Looks up a localized string similar to: No internet connection
        /// </summary>
        public static string InfoBarDatafileTitleNoInternet
        {
            get
            {
                return _resourceLoader.GetString("InfoBarDatafileTitleNoInternet");
            }
        }
        #endregion

        #region InfoBarDatafileTitleUpdated
        /// <summary>
        ///   Looks up a localized string similar to: Datafile updated
        /// </summary>
        public static string InfoBarDatafileTitleUpdated
        {
            get
            {
                return _resourceLoader.GetString("InfoBarDatafileTitleUpdated");
            }
        }
        #endregion

        #region InfoBarDatafileUpdated
        /// <summary>
        ///   Looks up a localized string similar to: The data file has been successfully updated.
        /// </summary>
        public static string InfoBarDatafileUpdated
        {
            get
            {
                return _resourceLoader.GetString("InfoBarDatafileUpdated");
            }
        }
        #endregion

        #region ShellPageWarnInfo
        /// <summary>
        ///   Looks up a localized string similar to: Screen recording protection not active
        /// </summary>
        public static string ShellPageWarnInfo
        {
            get
            {
                return _resourceLoader.GetString("ShellPageWarnInfo");
            }
        }
        #endregion

        #region AccountCodePageTitle
        /// <summary>
        ///   Looks up a localized string similar to: Accounts
        /// </summary>
        public static string AccountCodePageTitle
        {
            get
            {
                return _resourceLoader.GetString("AccountCodePageTitle");
            }
        }
        #endregion

        #region ApplicationName
        /// <summary>
        ///   Looks up a localized string similar to: [Beta] 2fast - two factor authenticator supporting TOTP
        /// </summary>
        public static string ApplicationName
        {
            get
            {
                return _resourceLoader.GetString("ApplicationName");
            }
        }
        #endregion

        #region NewDatafilePasswordInfo
        /// <summary>
        ///   Looks up a localized string similar to: A password must be assigned for the encryption of the data file. It is strongly recommended to store this password securely in a password manager.
        /// </summary>
        public static string NewDatafilePasswordInfo
        {
            get
            {
                return _resourceLoader.GetString("NewDatafilePasswordInfo");
            }
        }
        #endregion

        #region NewDatafilePasswordInfoTitle
        /// <summary>
        ///   Looks up a localized string similar to: Passwort für die Verschlüsselung
        /// </summary>
        public static string NewDatafilePasswordInfoTitle
        {
            get
            {
                return _resourceLoader.GetString("NewDatafilePasswordInfoTitle");
            }
        }
        #endregion

        #region TutorialPageItemCopyCodeBTNDesc
        /// <summary>
        ///   Looks up a localized string similar to: The key is copied to the clipboard.
        /// </summary>
        public static string TutorialPageItemCopyCodeBTNDesc
        {
            get
            {
                return _resourceLoader.GetString("TutorialPageItemCopyCodeBTNDesc");
            }
        }
        #endregion

        #region TutorialPageItemCopyCodeBTNTitle
        /// <summary>
        ///   Looks up a localized string similar to: Copy key
        /// </summary>
        public static string TutorialPageItemCopyCodeBTNTitle
        {
            get
            {
                return _resourceLoader.GetString("TutorialPageItemCopyCodeBTNTitle");
            }
        }
        #endregion

        #region TutorialPageItemFavouriteBTNTDesc
        /// <summary>
        ///   Looks up a localized string similar to: By setting a favourite, the account is displayed in the accent colour of the application and is placed on top in the display order.
        /// </summary>
        public static string TutorialPageItemFavouriteBTNTDesc
        {
            get
            {
                return _resourceLoader.GetString("TutorialPageItemFavouriteBTNTDesc");
            }
        }
        #endregion

        #region TutorialPageItemFavouriteBTNTitle
        /// <summary>
        ///   Looks up a localized string similar to: Set favourite
        /// </summary>
        public static string TutorialPageItemFavouriteBTNTitle
        {
            get
            {
                return _resourceLoader.GetString("TutorialPageItemFavouriteBTNTitle");
            }
        }
        #endregion

        #region TutorialPageItemMoreBTNDesc
        /// <summary>
        ///   Looks up a localized string similar to: Additional options for the account are displayed here
        /// </summary>
        public static string TutorialPageItemMoreBTNDesc
        {
            get
            {
                return _resourceLoader.GetString("TutorialPageItemMoreBTNDesc");
            }
        }
        #endregion

        #region TutorialPageItemMoreBTNTitle
        /// <summary>
        ///   Looks up a localized string similar to: More options
        /// </summary>
        public static string TutorialPageItemMoreBTNTitle
        {
            get
            {
                return _resourceLoader.GetString("TutorialPageItemMoreBTNTitle");
            }
        }
        #endregion

        #region TutorialPageItemShowCodeBTNDesc
        /// <summary>
        ///   Looks up a localized string similar to: Optionally, keys can be hidden and displayed again
        /// </summary>
        public static string TutorialPageItemShowCodeBTNDesc
        {
            get
            {
                return _resourceLoader.GetString("TutorialPageItemShowCodeBTNDesc");
            }
        }
        #endregion

        #region TutorialPageItemShowCodeBTNTitle
        /// <summary>
        ///   Looks up a localized string similar to: Hide/display key
        /// </summary>
        public static string TutorialPageItemShowCodeBTNTitle
        {
            get
            {
                return _resourceLoader.GetString("TutorialPageItemShowCodeBTNTitle");
            }
        }
        #endregion

        #region AddAccountContentDialogScreenclipNotFound
        /// <summary>
        ///   Looks up a localized string similar to: Snipping tool not found
        /// </summary>
        public static string AddAccountContentDialogScreenclipNotFound
        {
            get
            {
                return _resourceLoader.GetString("AddAccountContentDialogScreenclipNotFound");
            }
        }
        #endregion
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("DotNetPlus.ReswPlus", "2.1.3")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [MarkupExtensionReturnType(ReturnType = typeof(string))]
    public class ResourcesExtension : MarkupExtension
    {
        public enum KeyEnum
        {
            __Undefined = 0,
            AccountCodePageCopied,
            AccountCodePageItemMoreBTNToolTip,
            AccountCodePageViewModelDeleteAccept,
            AccountCodePageViewModelDeleteCancel,
            DeleteDatafileContentDialogDeleteDescription,
            DeleteDatafileContentDialogTitle,
            AccountCopyCodeButtonToolTip,
            AddAccountContentDialogQRCodeContentError,
            AddAccountContentDialogReadQRCodeTeachingTipInfo,
            Confirm,
            Delete,
            Edit,
            Error,
            LoginPagePasswordMismatch,
            No,
            SettingsActionRequiredRestart,
            SettingsFactoryResetMessage,
            SettingsGiveFeedbackText,
            SettingsRemoveDatafileMessage,
            ShellPageFeedbackNavigationToolTip,
            WindowsHelloLoginMessage,
            WindowsHelloPreferMessage,
            Yes,
            AccountCodePageRemainingSecondsToolTip,
            ButtonTextCancel,
            ButtonTextConfirm,
            ButtonTextNo,
            ButtonTextYes,
            RateAppContentDialogLaterButton,
            RateAppContentDialogNoButton,
            RateAppContentDialogQuestion,
            RateAppContentDialogYesButton,
            RateAppContentDialogTitle,
            AuthorizationFileSystemContentDialogDescription,
            AuthorizationFileSystemContentDialogPrimaryBTN,
            AuthorizationFileSystemContentDialogSecondaryBTN,
            AuthorizationFileSystemContentDialogTitle,
            AccountCodePageCopyCodeTeachingTip,
            ChangePassword,
            CloseApp,
            DeleteAccountContentDialogDescription,
            DeleteAccountContentDialogTitle,
            ErrorHandle,
            ErrorHandleDescription,
            PasswordInvalidHeader,
            SettingsQRScanningHeader,
            SettingsQRScanningSecondsRange,
            ExceptionDatafileNotFound,
            AccountCodePageABBLogoutToolTip,
            AccountCodePageAddEntryToolTip,
            AccountCodePageReloadDatafileToolTip,
            PasswordInvalidMessage,
            SettingsFactoryResetDialogTitle,
            AccountCodePageABBUndoDeleteToolTip,
            NewDatafileContentDialogDatafileExistsError,
            ErrorHandleSendError,
            FactoryReset,
            ChangeDatafilePath,
            AccountCodePageWrongTimeBTN,
            AccountCodePageWrongTimeContent,
            AccountCodePageWrongTimeTitle,
            SettingsDatafileChangePasswordInfoTitle,
            ShellPagePaneTitle,
            AccountCodePageAutoSuggestBoxPlaceholder,
            AccountCodePageSearchNotFound,
            SettingsDependencyGroupAssets,
            SettingsDependencyGroupPackages,
            RestartApp,
            ErrorCopyToClipboard,
            ErrorHandleDescriptionLastSession,
            NewAppFeaturesContent,
            NewAppFeaturesTitle,
            WelcomePageTitle,
            CreateDatafile,
            LoadDatafile,
            UseDatafilePasswordInfo,
            ToolTipShowHelp,
            SettingsAutoLogoutDesc,
            SettingsAutoLogoutMinutes,
            Close,
            AddAccountCodeContentDialogQRCodeHelp,
            DisplayQRCodeContentDialogTitle,
            AddAccountCodeContentDialogInputSecretKeyHelp,
            AccountCodePageTooltipDeleteFavourite,
            AccountCodePageTooltipSetFavourite,
            AccountCodePageTooltipHideTOTP,
            AccountCodePageTooltipShowTOTP,
            ButtonTextRetry,
            ErrorClipboardTask,
            AddAccountCodeContentDialogExpertSettingsHelp,
            AccountCodePageSearchFilterToolTip,
            WebViewDatafileContentDialogChooseFile,
            WebViewDatafileContentDialogChooseFolder,
            WelcomePageTutorialTipOpen,
            WelcomePageTutorialDesc,
            FunctionNotAvailable,
            WindowsHelloNotAvailable,
            ErrorGenerateTOTPCode,
            TextFormatterBoldToolTip,
            TextFormatterItalicToolTip,
            TextFormatterStrikeToolTip,
            TextFormatterUnderlineToolTip,
            TextFormatterListToolTip,
            TextFormatterOrderedToolTip,
            WriteDatafileErrorBTNCancel,
            WriteDatafileErrorBTNRetry,
            WriteDatafileErrorDesc,
            DatafileViewModelWebDAVServerNotFound,
            DatafileViewModelWebDAVServerNotFoundDesc,
            WebDAVAppPasswordInfo,
            DatafileViewModelWebDAVCredentialsError,
            DatafileViewModelWebDAVCredentialsErrorDesc,
            DatafileViewModelWebDAVMaintenanceError,
            DatafileViewModelWebDAVMaintenanceErrorDesc,
            DatafileViewModelWebDAVNotInstalledError,
            DatafileViewModelWebDAVNotInstalledErrorDesc,
            InfoBarDatafileNoInternet,
            InfoBarDatafileTitleNoInternet,
            InfoBarDatafileTitleUpdated,
            InfoBarDatafileUpdated,
            ShellPageWarnInfo,
            AccountCodePageTitle,
            ApplicationName,
            NewDatafilePasswordInfo,
            NewDatafilePasswordInfoTitle,
            TutorialPageItemCopyCodeBTNDesc,
            TutorialPageItemCopyCodeBTNTitle,
            TutorialPageItemFavouriteBTNTDesc,
            TutorialPageItemFavouriteBTNTitle,
            TutorialPageItemMoreBTNDesc,
            TutorialPageItemMoreBTNTitle,
            TutorialPageItemShowCodeBTNDesc,
            TutorialPageItemShowCodeBTNTitle,
        }

        private static ResourceLoader _resourceLoader;
        static ResourcesExtension()
        {
            _resourceLoader = ResourceLoader.GetForViewIndependentUse("Resources");
        }
        public KeyEnum Key { get; set; }
        public IValueConverter Converter { get; set; }
        public object ConverterParameter { get; set; }
        protected override object ProvideValue()
        {
            string res;
            if (Key == KeyEnum.__Undefined)
            {
                res = "";
            }
            else
            {
                res = _resourceLoader.GetString(Key.ToString());
            }
            return Converter == null ? res : Converter.Convert(res, typeof(String), ConverterParameter, null);
        }
    }
}
