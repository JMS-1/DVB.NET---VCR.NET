﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace JMS.DVB.Administration.ProfileManager.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("JMS.DVB.Administration.ProfileManager.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0}\r\nWhat do you want to do?.
        /// </summary>
        internal static string Convert_AskUser {
            get {
                return ResourceManager.GetString("Convert_AskUser", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Upgrade of profile &apos;{0}&apos; failed.
        /// </summary>
        internal static string Convert_ErrorTitle {
            get {
                return ResourceManager.GetString("Convert_ErrorTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Upgrade pre-3.9 DVB.NET Profiles into the current Format.
        /// </summary>
        internal static string Convert_Long {
            get {
                return ResourceManager.GetString("Convert_Long", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Profile Upgrade.
        /// </summary>
        internal static string Convert_Short {
            get {
                return ResourceManager.GetString("Convert_Short", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to delete Profile.
        /// </summary>
        internal static string DeleteProfile_Error {
            get {
                return ResourceManager.GetString("DeleteProfile_Error", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Delete an existing Profile permanently.
        /// </summary>
        internal static string DeleteProfile_Long {
            get {
                return ResourceManager.GetString("DeleteProfile_Long", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Delete Profile.
        /// </summary>
        internal static string DeleteProfile_Short {
            get {
                return ResourceManager.GetString("DeleteProfile_Short", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Configuration of the Profile is incomplete - do you really want to save it?.
        /// </summary>
        internal static string EditProfile_Invalid {
            get {
                return ResourceManager.GetString("EditProfile_Invalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Select an existing DVB.NET Profile and change its configuration (Profile Management).
        /// </summary>
        internal static string EditProfile_Long {
            get {
                return ResourceManager.GetString("EditProfile_Long", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Profile Editor.
        /// </summary>
        internal static string EditProfile_Short {
            get {
                return ResourceManager.GetString("EditProfile_Short", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to create profile.
        /// </summary>
        internal static string NewProfile_Error {
            get {
                return ResourceManager.GetString("NewProfile_Error", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Create a new Profile.
        /// </summary>
        internal static string NewProfile_Long {
            get {
                return ResourceManager.GetString("NewProfile_Long", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Create Profile.
        /// </summary>
        internal static string NewProfile_Short {
            get {
                return ResourceManager.GetString("NewProfile_Short", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Legacy DVB.NET Hardware Abstraction (prior to Version 4.0).
        /// </summary>
        internal static string Type_Legacy {
            get {
                return ResourceManager.GetString("Type_Legacy", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DVB.NET BDA Hardware Abstraction (beginning with Version 4.0).
        /// </summary>
        internal static string Type_Standard {
            get {
                return ResourceManager.GetString("Type_Standard", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to (private Sources).
        /// </summary>
        internal static string UseFrom_Self {
            get {
                return ResourceManager.GetString("UseFrom_Self", resourceCulture);
            }
        }
    }
}
