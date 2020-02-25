﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Jeno {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Messages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Messages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Jeno.Messages", typeof(Messages).Assembly);
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
        ///   Looks up a localized string similar to Use &quot;jeno help&quot; command to get more information.
        /// </summary>
        internal static string BasicMessage {
            get {
                return ResourceManager.GetString("BasicMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Set options of app configuration.
        /// </summary>
        internal static string ChangeConfigurationDescription {
            get {
                return ResourceManager.GetString("ChangeConfigurationDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use &quot;jeno set repository:default=[defaultJob]&quot; command to save default job.
        /// </summary>
        internal static string ConfigureDefaultJobTip {
            get {
                return ResourceManager.GetString("ConfigureDefaultJobTip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use &quot;jeno set jenkinsUrl:[url]&quot; command to save correct Jenkins address.
        /// </summary>
        internal static string ConfigureJenkinsAddressTip {
            get {
                return ResourceManager.GetString("ConfigureJenkinsAddressTip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use &quot;jeno set token:[token]&quot; command to save authorization token.
        /// </summary>
        internal static string ConfigureTokenTip {
            get {
                return ResourceManager.GetString("ConfigureTokenTip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use &quot;jeno set userName:[username]&quot; command to save login.
        /// </summary>
        internal static string ConfigureUserNameTip {
            get {
                return ResourceManager.GetString("ConfigureUserNameTip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot get crumb for CSRF protection system:.
        /// </summary>
        internal static string CsrfException {
            get {
                return ResourceManager.GetString("CsrfException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Remove selected repository from configuration.
        /// </summary>
        internal static string DeleteRepoOptionDescription {
            get {
                return ResourceManager.GetString("DeleteRepoOptionDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to List of available commands.
        /// </summary>
        internal static string HelpCommandDescription {
            get {
                return ResourceManager.GetString("HelpCommandDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Jenkins address is undefined or incorrect.
        /// </summary>
        internal static string IncorrectJenkinsAddress {
            get {
                return ResourceManager.GetString("IncorrectJenkinsAddress", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Some of job parameters have incorrect format.
        /// </summary>
        internal static string IncorrectJobParameters {
            get {
                return ResourceManager.GetString("IncorrectJobParameters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Jeno is a command line interface allows to running Jenkins jobs.
        /// </summary>
        internal static string JenoDescription {
            get {
                return ResourceManager.GetString("JenoDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot run job: .
        /// </summary>
        internal static string JobException {
            get {
                return ResourceManager.GetString("JobException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Missing default job.
        /// </summary>
        internal static string MissingDefaultJob {
            get {
                return ResourceManager.GetString("MissingDefaultJob", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Some of passed repositories have undefined name.
        /// </summary>
        internal static string MissingRepoName {
            get {
                return ResourceManager.GetString("MissingRepoName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Current directory is not git repository..
        /// </summary>
        internal static string NotGitRepo {
            get {
                return ResourceManager.GetString("NotGitRepo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot remove default job.
        /// </summary>
        internal static string RemoveDefaultJobException {
            get {
                return ResourceManager.GetString("RemoveDefaultJobException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to List of parameters for running job.
        /// </summary>
        internal static string RunJobArgumentsDescription {
            get {
                return ResourceManager.GetString("RunJobArgumentsDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Run job on Jenkins.
        /// </summary>
        internal static string RunJobDescription {
            get {
                return ResourceManager.GetString("RunJobDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Save passed settings in app configuration.
        /// </summary>
        internal static string SettingDescription {
            get {
                return ResourceManager.GetString("SettingDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Show Jeno configuration.
        /// </summary>
        internal static string ShowConfigurationCommandDescription {
            get {
                return ResourceManager.GetString("ShowConfigurationCommandDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to User token is undefined.
        /// </summary>
        internal static string UndefinedToken {
            get {
                return ResourceManager.GetString("UndefinedToken", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Username is undefined.
        /// </summary>
        internal static string UndefinedUserName {
            get {
                return ResourceManager.GetString("UndefinedUserName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unsupported parameter:.
        /// </summary>
        internal static string UnsupportedSetting {
            get {
                return ResourceManager.GetString("UnsupportedSetting", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Some of passed options have unhandled format:.
        /// </summary>
        internal static string WrongConfigurationParametersFormat {
            get {
                return ResourceManager.GetString("WrongConfigurationParametersFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Some of passed repositories have unhandled format.
        /// </summary>
        internal static string WrongReposFormat {
            get {
                return ResourceManager.GetString("WrongReposFormat", resourceCulture);
            }
        }
    }
}
