using System.ComponentModel;
using HtmlAgilityPack;

namespace JiraStoryPrint
{
    class JiraStoryUpdater : INotifyPropertyChanged
    {
        private const int StoryHeaderFontSize = 14;
        private const int StoryTextFontSize = 12;
        private const int CommentsTextFontSize = 10;

        /// <summary>
        /// Gets the HTML document.
        /// </summary>
        public HtmlDocument Html { get; private set; }

        /// <summary>
        /// Gets a value indicating whether HTML-file is opened.
        /// </summary>
        /// <value>
        /// <c>true</c> if HTML-file is opened; otherwise, <c>false</c>.
        /// </value>
        public bool FileIsOpened
        {
            get
            {
                return Html != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether file is processed.
        /// </summary>
        /// <value>
        /// <c>true</c> if file is processed; otherwise, <c>false</c>.
        /// </value>
        public bool FileIsProcessed { get; private set; }

        /// <summary>
        /// Opens the file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public void OpenFile(string fileName)
        {
            Html = new HtmlDocument();
            Html.Load(fileName);
            
            FileIsProcessed = false;
            invokePropertyChanged(new PropertyChangedEventArgs(string.Empty));
        }

        /// <summary>
        /// Processes the file by updating file text.
        /// </summary>
        public void ProcessFile()
        {
            var pageBody = Html.DocumentNode.SelectSingleNode("//body");
            var newDocNodes = new HtmlNodeCollection(pageBody);

            // Set white background for body.
            pageBody.Attributes.Add("style", "background-color:#fff;");

            // Process stories.
            var stories = pageBody.SelectNodes("table[@class='tableBorder']");
            foreach (var story in stories)
            {
                story.Attributes.Add("style", string.Format("font-size:{0}pt;", StoryTextFontSize));
                var storyBody = story.SelectSingleNode("tbody") ?? story;

                // Story header.
                var header = storyBody.SelectSingleNode("tr");
                var headerCell = header.SelectSingleNode("td");
                headerCell.Attributes["colspan"].Value = "4";
                headerCell.Attributes.Add("style", string.Format("font-size:{0}pt;", StoryHeaderFontSize));
                headerCell.SelectSingleNode("h3").Attributes.Remove("class");
                headerCell.SelectSingleNode("h3/span").Remove();

                storyBody.RemoveAllChildren();
                storyBody.AppendChild(header);

                // Story details.
                var details1 = story.SelectSingleNode("following-sibling::table[1]/tbody") ??
                               story.SelectSingleNode("following-sibling::table[1]");
                var reporter = details1.SelectSingleNode("tr[contains(., 'Reporter')]");
                var estimate = details1.SelectSingleNode("tr[contains(., 'Original Estimate')]");
                
                if (reporter != null)
                {
                    reporter.InnerHtml = reporter.InnerHtml.Replace("Unassigned", string.Empty);
                    storyBody.AppendChild(reporter);
                }

                if (estimate != null)
                {
                    estimate.InnerHtml = estimate.InnerHtml.Replace("Not Specified", string.Empty);
                    estimate.InnerHtml = estimate.InnerHtml.Replace("Original ", string.Empty);
                    storyBody.AppendChild(estimate);
                }

                var details2 = story.SelectSingleNode("following-sibling::table[2]/tbody") ??
                               story.SelectSingleNode("following-sibling::table[2]");
                var themes = details2.SelectSingleNode("tr[contains(., 'Theme/s')]");
                var acceptanceCriteria = details2.SelectSingleNode("tr[contains(., 'Acceptance Criteria')]");
                
                if (themes != null)
                {
                    themes.SelectSingleNode("td[2]").Attributes.Add("colspan", "3");
                    storyBody.AppendChild(themes);
                }

                if (acceptanceCriteria != null)
                {
                    acceptanceCriteria.SelectSingleNode("td[1]").Attributes["bgcolor"].Remove();
                    acceptanceCriteria.SelectSingleNode("td[2]").Attributes.Add("colspan", "3");
                    storyBody.AppendChild(acceptanceCriteria);
                }

                // Set previous element to find optional description and comments tables.
                var previousElement = details2.Name == "table" ? details2 : details2.ParentNode;

                // Description.
                var descriptionHeader = previousElement.SelectSingleNode("following-sibling::table[1]/tbody/tr/td") ??
                                        previousElement.SelectSingleNode("following-sibling::table[1]/tr/td");
                if (descriptionHeader != null && descriptionHeader.InnerText.Trim() == "&nbsp;Description&nbsp;")
                {
                    previousElement = previousElement.SelectSingleNode("following-sibling::table[2]");
                    var description = previousElement.SelectSingleNode("tbody/tr/td") ??
                                      previousElement.SelectSingleNode("tr/td");
                    description.Attributes.Remove("id");
                    description.Attributes.Add("colspan", "4");
                    storyBody.AppendChild(description.ParentNode);
                }

                // Comments.
                var commentsHeader = previousElement.SelectSingleNode("following-sibling::table[1]/tbody/tr/td") ??
                                     previousElement.SelectSingleNode("following-sibling::table[1]/tr/td");
                if (commentsHeader != null && commentsHeader.InnerText.Trim() == "&nbsp;Comments&nbsp;")
                {
                    var comments = previousElement.SelectNodes("following-sibling::table[2]/tbody/tr") ??
                                   previousElement.SelectNodes("following-sibling::table[2]/tr");
                    foreach (var comment in comments)
                    {
                        if (comment.Attributes["id"].Value.StartsWith("comment-header"))
                        {
                            comment.SelectSingleNode("td/font").Remove();
                        }

                        comment.SelectSingleNode("td").Attributes.Add("colspan", "4");
                        comment.Attributes.Add("style", string.Format("font-size:{0}pt;", CommentsTextFontSize));
                    }

                    storyBody.AppendChildren(comments);
                }

                // Save new story.
                newDocNodes.Append(story);
                newDocNodes.Append(Html.CreateTextNode("<br><br><br><br><br>"));
            }

            pageBody.RemoveAllChildren();
            pageBody.AppendChildren(newDocNodes);

            FileIsProcessed = true;
            invokePropertyChanged(new PropertyChangedEventArgs("FileIsProcessed"));
        }

        public void Reset()
        {
            Html = null;
            FileIsProcessed = false;
            invokePropertyChanged(new PropertyChangedEventArgs(string.Empty));
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private void invokePropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
    }
}
