//----------------------------------------------------------------------------
// File Name: Log.cs
// 
// Description: 
// Log provides a one stop shop for log settings rather then have configuration 
// functions in separate classes.
//
// Author(s):
// Aaron Clauson
//
// History:
// 04 Nov 2004	Aaron Clauson   Created.
// 14 Sep 2019  Aaron Clauson   Added NetStandard support.
//
// License:
// BSD 3-Clause "New" or "Revised" License, see included LICENSE.md file.
//----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using SQ.Base;
using System;
using System.Diagnostics;

namespace GBWeb.Attribute
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthApiAttribute : System.Attribute
    {

    } 
}
