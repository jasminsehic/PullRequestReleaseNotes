//
// Summary:
//     Space information See: https://docs.atlassian.com/confluence/REST/latest

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UnreleasedGitHubHistory.Models
{
    [DataContract]
    public class Content
    {
        //
        // Summary:
        //     Body of the content
        [DataMember(Name = "body")]
        public Body Body { get; set; }

        //
        // Summary:
        //     The values that are expandable
        [DataMember(Name = "_expandable")]
        public IDictionary<string, string> Expandables { get; set; }

        //
        // Summary:
        //     Unique ID for the content
        [DataMember(Name = "id")]
        public string Id { get; set; }

        //
        // Summary:
        //     Different links for this entity, depending on the entry
        [DataMember(Name = "_links")]
        public Links Links { get; set; }

        //
        // Summary:
        //     The space where this content is
        [DataMember(Name = "space")]
        public Space Space { get; set; }

        //
        // Summary:
        //     The title of the content
        [DataMember(Name = "title")]
        public string Title { get; set; }

        //
        // Summary:
        //     The type for the content, e.g. page
        [DataMember(Name = "type")]
        public string Type { get; set; }

        //
        // Summary:
        //     Version information for the content, this is not filled unless expand=version
        [DataMember(Name = "version")]
        public ContentVersion Version { get; set; }

        //
        // Summary:
        //     List of content ancestors
        [DataMember(Name = "ancestors")]
        public Ancestor[] Ancestors { get; set; }
    }

    [DataContract]
    public class ContentVersion
    {
        //
        // Summary:
        //     Content version nu mber
        [DataMember(Name = "number")]
        public int Number { get; set; }
    }

    [DataContract]
    public class ContentResults
    {
        [DataMember(Name = "results")]
        public Result[] Results { get; set; }

        [DataMember(Name = "start")]
        public int Start { get; set; }

        [DataMember(Name = "limit")]
        public int Limit { get; set; }

        [DataMember(Name = "size")]
        public int Size { get; set; }
    }

    [DataContract]
    public class Result
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "status")]
        public string Status { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }
    }

    /// <summary>
    ///     Body
    ///     See: https://docs.atlassian.com/confluence/REST/latest
    /// </summary>
    [DataContract]
    public class Body
    {
        /// <summary>
        /// View for Body
        /// </summary>
        [DataMember(Name = "view")]
        public BodyContent View { get; set; }

        /// <summary>
        /// Storage for content, used when creating
        /// </summary>
        [DataMember(Name = "storage")]
        public BodyContent Storage { get; set; }
    }

    /// <summary>
    ///     Content for the body
    /// </summary>
    [DataContract]
    public class BodyContent
    {
        /// <summary>
        ///     Representation
        /// </summary>
        [DataMember(Name = "representation")]
        public string Representation { get; set; }

        /// <summary>
        ///     Value of the view
        /// </summary>
        [DataMember(Name = "value")]
        public string Value { get; set; }
    }

    /// <summary>
    ///     Links information
    ///     See: https://docs.atlassian.com/confluence/REST/latest
    /// </summary>
    [DataContract]
    public class Links
    {
        /// <summary>
        /// The base (hostname) for the server
        /// </summary>
        [DataMember(Name = "base")]
        public Uri Base { get; set; }

        /// <summary>
        /// A path to the rest API to where this belongs, content has a collection of "/rest/api/content"
        /// </summary>
        [DataMember(Name = "collection")]
        public string Collection { get; set; }

        /// <summary>
        /// TODO: What is this?
        /// </summary>
        [DataMember(Name = "context")]
        public string Context { get; set; }

        /// <summary>
        /// The link, usually for attachments, to download the content
        /// </summary>
        [DataMember(Name = "download")]
        public string Download { get; set; }

        /// <summary>
        /// A link to the entity itself (so one can find it again)
        /// </summary>
        [DataMember(Name = "self")]
        public Uri Self { get; set; }

        /// <summary>
        /// A short link to the content, relative to the hostname (and port)
        /// </summary>
        [DataMember(Name = "tinyui")]
        public string TinyUi { get; set; }

        /// <summary>
        /// A normal, but well readable, link to the content
        /// </summary>
        [DataMember(Name = "webui")]
        public string WebUi { get; set; }
    }

    /// <summary>
    ///     Space information
    ///     See: https://docs.atlassian.com/confluence/REST/latest
    ///     Should be called with expand=icon,description.plain,homepage
    /// </summary>
    [DataContract]
    public class Space
    {
        /// <summary>
        /// The values that are expandable
        /// </summary>
        [DataMember(Name = "_expandable")]
        public IDictionary<string, string> Expandables { get; set; }

        /// <summary>
        /// Icon for the space
        /// </summary>
        [DataMember(Name = "icon")]
        public Picture Icon { get; set; }

        /// <summary>
        /// Id for the space
        /// </summary>
        [DataMember(Name = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Test if this space is a personal space, this is true when the Key starts with a ~
        /// </summary>
        public bool IsPersonal => true == Key?.StartsWith("~");

        /// <summary>
        /// Key for the space
        /// </summary>
        [DataMember(Name = "key")]
        public string Key { get; set; }

        /// <summary>
        /// Different links for this entity, depending on the entry
        /// </summary>
        [DataMember(Name = "_links")]
        public Links Links { get; set; }

        /// <summary>
        /// The name of the space
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Type for the space, e.g. Team space or Knowledge Base space etc
        /// </summary>
        [DataMember(Name = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [DataMember(Name = "description")]
        public Description Description { get; set; }
    }

    /// <summary>
    ///     Description information
    ///     See: https://docs.atlassian.com/confluence/REST/latest
    /// </summary>
    [DataContract]
    public class Description
    {
        /// <summary>
        /// Plain text
        /// </summary>
        [DataMember(Name = "plain")]
        public Plain Plain { get; set; }
    }

    /// <summary>
    ///     Plain information, used in the description.
    /// TODO: Find a better name
    ///     See: https://docs.atlassian.com/confluence/REST/latest
    /// </summary>
    [DataContract]
    public class Plain
    {
        /// <summary>
        /// Value of the plain description
        /// </summary>
        [DataMember(Name = "value")]
        public string Value { get; set; }

        /// <summary>
        /// Type of representation
        /// </summary>
        [DataMember(Name = "representation")]
        public string Representation { get; set; } = "plain";
    }

    /// <summary>
    ///     Space information
    ///     See: https://docs.atlassian.com/confluence/REST/latest
    /// </summary>
    [DataContract]
    public class Picture
    {
        /// <summary>
        /// Height of the picture
        /// </summary>
        [DataMember(Name = "height")]
        public int Height { get; set; }

        /// <summary>
        /// Is this picture the default
        /// </summary>
        [DataMember(Name = "isDefault")]
        public bool IsDefault { get; set; }

        /// <summary>
        /// The path for the picture relative to the hostname (and port) of the server, this is outside the Rest API path
        /// </summary>
        [DataMember(Name = "path")]
        public string Path { get; set; }

        /// <summary>
        /// Width of the picture
        /// </summary>
        [DataMember(Name = "width")]
        public int Width { get; set; }
    }

    [DataContract]
    public class Ancestor
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }
        [DataMember(Name = "type")]
        public string Type { get; set; }
    }
}