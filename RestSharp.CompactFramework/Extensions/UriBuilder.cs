// Decompiled with JetBrains decompiler
// Type: System.UriBuilder
// Assembly: System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// MVID: B1F13997-B8CA-4245-919B-2647D0459836
// Assembly location: C:\Windows\Microsoft.NET\Framework\v2.0.50727\System.dll

using System.Globalization;
using System.Net;
using System.Text;
using System.Threading;

namespace System
{
    /// <summary>Provides a custom constructor for uniform resource identifiers (URIs) and modifies URIs for the <see cref="T:System.Uri" /> class.</summary>
    /// <filterpriority>2</filterpriority>
    public class UriBuilder
    {
        private bool m_changed = true;
        private string m_fragment = string.Empty;
        private string m_host = "localhost";
        private string m_password = string.Empty;
        private string m_path = "/";
        private int m_port = -1;
        private string m_query = string.Empty;
        private string m_scheme = "http";
        private string m_schemeDelimiter = Uri.SchemeDelimiter;
        private string m_username = string.Empty;
        private Uri m_uri;

        /// <summary>Initializes a new instance of the <see cref="T:System.UriBuilder" /> class.</summary>
        public UriBuilder()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.UriBuilder" /> class with the specified URI.</summary>
        /// <param name="uri">A URI string. </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="uri" /> is null. </exception>
        /// <exception cref="T:System.UriFormatException">In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.FormatException" />, instead.<paramref name="uri" /> is a zero length string or contains only spaces.-or- The parsing routine detected a scheme in an invalid form.-or- The parser detected more than two consecutive slashes in a URI that does not use the "file" scheme.-or- <paramref name="uri" /> is not a valid URI. </exception>
        public UriBuilder(string uri)
        {
            Uri uri1 = new Uri(uri, UriKind.RelativeOrAbsolute);
            if (uri1.IsAbsoluteUri) {
                this.Init(uri1);
            } else {
                uri = Uri.UriSchemeHttp + Uri.SchemeDelimiter + uri;
                this.Init(new Uri(uri));
            }
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.UriBuilder" /> class with the specified <see cref="T:System.Uri" /> instance.</summary>
        /// <param name="uri">An instance of the <see cref="T:System.Uri" /> class. </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="uri" /> is null. </exception>
        public UriBuilder(Uri uri)
        {
            this.Init(uri);
        }

        private void Init(Uri uri)
        {
            this.m_fragment = uri.Fragment;
            this.m_query = uri.Query;
            this.m_host = uri.Host;
            this.m_path = uri.AbsolutePath;
            this.m_port = uri.Port;
            this.m_scheme = uri.Scheme;
            this.m_schemeDelimiter = Uri.SchemeDelimiter;
            string userInfo = uri.UserInfo;
            if (!string.IsNullOrEmpty(userInfo)) {
                int length = userInfo.IndexOf(':');
                if (length != -1) {
                    this.m_password = userInfo.Substring(length + 1);
                    this.m_username = userInfo.Substring(0, length);
                } else
                    this.m_username = userInfo;
            }
            this.SetFieldsFromUri(uri);
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.UriBuilder" /> class with the specified scheme and host.</summary>
        /// <param name="schemeName">An Internet access protocol. </param>
        /// <param name="hostName">A DNS-style domain name or IP address. </param>
        public UriBuilder(string schemeName, string hostName)
        {
            this.Scheme = schemeName;
            this.Host = hostName;
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.UriBuilder" /> class with the specified scheme, host, and port.</summary>
        /// <param name="scheme">An Internet access protocol. </param>
        /// <param name="host">A DNS-style domain name or IP address. </param>
        /// <param name="portNumber">An IP port number for the service. </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="portNumber" /> is less than -1 or greater than 65,535. </exception>
        public UriBuilder(string scheme, string host, int portNumber)
            : this(scheme, host)
        {
            this.Port = portNumber;
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.UriBuilder" /> class with the specified scheme, host, port number, and path.</summary>
        /// <param name="scheme">An Internet access protocol. </param>
        /// <param name="host">A DNS-style domain name or IP address. </param>
        /// <param name="port">An IP port number for the service. </param>
        /// <param name="pathValue">The path to the Internet resource. </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="port" /> is less than -1 or greater than 65,535. </exception>
        public UriBuilder(string scheme, string host, int port, string pathValue)
            : this(scheme, host, port)
        {
            this.Path = pathValue;
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.UriBuilder" /> class with the specified scheme, host, port number, path and query string or fragment identifier.</summary>
        /// <param name="scheme">An Internet access protocol. </param>
        /// <param name="host">A DNS-style domain name or IP address. </param>
        /// <param name="port">An IP port number for the service. </param>
        /// <param name="path">The path to the Internet resource. </param>
        /// <param name="extraValue">A query string or fragment identifier. </param>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="extraValue" /> is neither null nor <see cref="F:System.String.Empty" />, nor does a valid fragment identifier begin with a number sign (#), nor a valid query string begin with a question mark (?). </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="port" /> is less than -1 or greater than 65,535. </exception>
        public UriBuilder(string scheme, string host, int port, string path, string extraValue)
            : this(scheme, host, port, path)
        {
            try {
                this.Extra = extraValue;
            } catch (Exception ex) {
                if (!(ex is ThreadAbortException) && !(ex is StackOverflowException) && !(ex is OutOfMemoryException))
                    throw new ArgumentException("extraValue");
                throw;
            }
        }

        private string Extra
        {
            set
            {
                if (value == null)
                    value = string.Empty;
                if (value.Length > 0) {
                    if ((int)value[0] == 35) {
                        this.Fragment = value.Substring(1);
                    } else {
                        if ((int)value[0] != 63)
                            throw new ArgumentException("value");
                        int num = value.IndexOf('#');
                        if (num == -1)
                            num = value.Length;
                        else
                            this.Fragment = value.Substring(num + 1);
                        this.Query = value.Substring(1, num - 1);
                    }
                } else {
                    this.Fragment = string.Empty;
                    this.Query = string.Empty;
                }
            }
        }

        /// <summary>Gets or sets the fragment portion of the URI.</summary>
        /// <returns>The fragment portion of the URI. The fragment identifier ("#") is added to the beginning of the fragment.</returns>
        /// <filterpriority>2</filterpriority>
        public string Fragment
        {
            get
            {
                return this.m_fragment;
            }
            set
            {
                if (value == null)
                    value = string.Empty;
                if (value.Length > 0)
                    value = 35.ToString() + value;
                this.m_fragment = value;
                this.m_changed = true;
            }
        }

        /// <summary>Gets or sets the Domain Name System (DNS) host name or IP address of a server.</summary>
        /// <returns>The DNS host name or IP address of the server.</returns>
        /// <filterpriority>1</filterpriority>
        public string Host
        {
            get
            {
                return this.m_host;
            }
            set
            {
                if (value == null)
                    value = string.Empty;
                this.m_host = value;
                if (this.m_host.IndexOf(':') >= 0 && (int)this.m_host[0] != 91)
                    this.m_host = "[" + this.m_host + "]";
                this.m_changed = true;
            }
        }

        /// <summary>Gets or sets the password associated with the user that accesses the URI.</summary>
        /// <returns>The password of the user that accesses the URI.</returns>
        /// <filterpriority>1</filterpriority>
        public string Password
        {
            get
            {
                return this.m_password;
            }
            set
            {
                if (value == null)
                    value = string.Empty;
                this.m_password = value;
            }
        }

        /// <summary>Gets or sets the path to the resource referenced by the URI.</summary>
        /// <returns>The path to the resource referenced by the URI.</returns>
        /// <filterpriority>1</filterpriority>
        public string Path
        {
            get
            {
                return this.m_path;
            }
            set
            {
                if (value == null || value.Length == 0)
                    value = "/";
                this.m_path = this.ConvertSlashes(value);
                this.m_changed = true;
            }
        }

        /// <summary>Gets or sets the port number of the URI.</summary>
        /// <returns>The port number of the URI.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The port cannot be set to a value less than -1 or greater than 65,535. </exception>
        /// <filterpriority>1</filterpriority>
        public int Port
        {
            get
            {
                return this.m_port;
            }
            set
            {
                if (value < -1 || value > (int)ushort.MaxValue)
                    throw new ArgumentOutOfRangeException("value");
                this.m_port = value;
                this.m_changed = true;
            }
        }

        /// <summary>Gets or sets any query information included in the URI.</summary>
        /// <returns>The query information included in the URI.</returns>
        /// <filterpriority>1</filterpriority>
        public string Query
        {
            get
            {
                return this.m_query;
            }
            set
            {
                if (value == null)
                    value = string.Empty;
                if (value.Length > 0)
                    value = 63.ToString() + value;
                this.m_query = value;
                this.m_changed = true;
            }
        }

        /// <summary>Gets or sets the scheme name of the URI.</summary>
        /// <returns>The scheme of the URI.</returns>
        /// <exception cref="T:System.ArgumentException">The scheme cannot be set to an invalid scheme name. </exception>
        /// <filterpriority>1</filterpriority>
        public string Scheme
        {
            get
            {
                return this.m_scheme;
            }
            set
            {
                if (value == null)
                    value = string.Empty;
                int length = value.IndexOf(':');
                if (length != -1)
                    value = value.Substring(0, length);
                if (value.Length != 0) {
                    if (!Uri.CheckSchemeName(value))
                        throw new ArgumentException("value");
                    value = value.ToLower(CultureInfo.InvariantCulture);
                }
                this.m_scheme = value;
                this.m_changed = true;
            }
        }

        /// <summary>Gets the <see cref="T:System.Uri" /> instance constructed by the specified <see cref="T:System.UriBuilder" /> instance.</summary>
        /// <returns>A <see cref="T:System.Uri" /> that contains the URI constructed by the <see cref="T:System.UriBuilder" />.</returns>
        /// <exception cref="T:System.UriFormatException">In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.FormatException" />, instead.The URI constructed by the <see cref="T:System.UriBuilder" /> properties is invalid. </exception>
        /// <filterpriority>1</filterpriority>
        public Uri Uri
        {
            get
            {
                if (this.m_changed) {
                    this.m_uri = new Uri(this.ToString());
                    this.SetFieldsFromUri(this.m_uri);
                    this.m_changed = false;
                }
                return this.m_uri;
            }
        }

        /// <summary>The user name associated with the user that accesses the URI.</summary>
        /// <returns>The user name of the user that accesses the URI.</returns>
        /// <filterpriority>1</filterpriority>
        public string UserName
        {
            get
            {
                return this.m_username;
            }
            set
            {
                if (value == null)
                    value = string.Empty;
                this.m_username = value;
            }
        }

        private string ConvertSlashes(string path)
        {
            StringBuilder stringBuilder = new StringBuilder(path.Length);
            for (int index = 0; index < path.Length; ++index) {
                char ch = path[index];
                if ((int)ch == 92)
                    ch = '/';
                stringBuilder.Append(ch);
            }
            return stringBuilder.ToString();
        }

        /// <summary>Compares an existing <see cref="T:System.Uri" /> instance with the contents of the <see cref="T:System.UriBuilder" /> for equality.</summary>
        /// <returns>true if <paramref name="rparam" /> represents the same <see cref="T:System.Uri" /> as the <see cref="T:System.Uri" /> constructed by this <see cref="T:System.UriBuilder" /> instance; otherwise, false.</returns>
        /// <param name="rparam">The object to compare with the current instance. </param>
        /// <filterpriority>2</filterpriority>
        public override bool Equals(object rparam)
        {
            if (rparam == null)
                return false;
            return this.Uri.Equals((object)rparam.ToString());
        }

        /// <summary>Returns the hash code for the URI.</summary>
        /// <returns>The hash code generated for the URI.</returns>
        /// <filterpriority>2</filterpriority>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        /// </PermissionSet>
        public override int GetHashCode()
        {
            return this.Uri.GetHashCode();
        }

        private void SetFieldsFromUri(Uri uri)
        {
            this.m_fragment = uri.Fragment;
            this.m_query = uri.Query;
            this.m_host = uri.Host;
            this.m_path = uri.AbsolutePath;
            this.m_port = uri.Port;
            this.m_scheme = uri.Scheme;
            this.m_schemeDelimiter = Uri.SchemeDelimiter;
            string userInfo = uri.UserInfo;
            if (userInfo.Length <= 0)
                return;
            int length = userInfo.IndexOf(':');
            if (length != -1) {
                this.m_password = userInfo.Substring(length + 1);
                this.m_username = userInfo.Substring(0, length);
            } else
                this.m_username = userInfo;
        }

        /// <summary>Returns the display string for the specified <see cref="T:System.UriBuilder" /> instance.</summary>
        /// <returns>The string that contains the unescaped display string of the <see cref="T:System.UriBuilder" />.</returns>
        /// <exception cref="T:System.UriFormatException">In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.FormatException" />, instead.The <see cref="T:System.UriBuilder" /> instance has a bad password. </exception>
        /// <filterpriority>1</filterpriority>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
        /// </PermissionSet>
        public override string ToString()
        {
            if (this.m_username.Length == 0 && this.m_password.Length > 0)
                throw new UriFormatException("net_uri_BadUserPassword");
            return (this.m_scheme.Length != 0 ? this.m_scheme + this.m_schemeDelimiter : string.Empty) + this.m_username + (this.m_password.Length > 0 ? ":" + this.m_password : string.Empty) + (this.m_username.Length > 0 ? "@" : string.Empty) + this.m_host + (this.m_port == -1 || this.m_host.Length <= 0 ? string.Empty : ":" + (object)this.m_port) + (this.m_host.Length <= 0 || this.m_path.Length == 0 || (int)this.m_path[0] == 47 ? string.Empty : "/") + this.m_path + this.m_query + this.m_fragment;
        }
    }
}
