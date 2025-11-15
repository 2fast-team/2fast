using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;

namespace Project2FA.Shared.Models
{
    public enum BackupServiceEnum
    {
        None = -1,
        [Description("Aegis")]
        Aegis = 0,
        [Description("AndOTP")]
        AndOTP = 1,
        [Description("2FAS")]
        TwoFAS = 2,
        [Description("2fast")]
        Twofast = 3,

    }
}