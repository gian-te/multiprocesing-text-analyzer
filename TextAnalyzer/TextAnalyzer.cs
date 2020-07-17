using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TextAnalyzer
{
    public class TextStatistics : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private long wordCount;
        public long WordCount
        {
            get { return wordCount; }
            set
            {
                wordCount = value;
                OnPropertyChanged("WordCount");
            }
        }

        private long sentenceCount;
        public long SentenceCount
        {
            get { return sentenceCount; }
            set
            {
                sentenceCount = value;
                OnPropertyChanged("SentenceCount");
            }
        }
        private long characterCount;
        public long CharacterCount
        {
            get { return characterCount; }
            set
            {
                characterCount = value;
                OnPropertyChanged("CharacterCount");
            }
        }
        private TimeSpan duration;
        public TimeSpan Duration
        {
            get { return duration; }
            set
            {
                duration = value;
                OnPropertyChanged("Duration");
            }
        }


        #region PROPERTY CHANGED HANDLER FOR REAL-TIME NOTIFICATION 
        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
    public interface ITextAnalyzer
    {
        void AnalyzeText(string text);
    }

    public class SingleThreadedTextAnalyzer : ITextAnalyzer, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public TextStatistics stats;
        public TextStatistics Stats
        {
            get
            {
                return stats;
            }
            set
            {
                stats = value;
                OnPropertyChanged("Stats");
            }
        }
        public SingleThreadedTextAnalyzer()
        {
            Stats = new TextStatistics();
        }

        public void AnalyzeText(string text)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            var sentences = text.Split(new char[] { '.', '?', '!' });

            foreach (var sentence in sentences)
            {
                if (!string.IsNullOrWhiteSpace(sentence))
                {
                    Stats.SentenceCount++;
                }
            }

            var escapedText = text.Replace("\r\n", "");
            var words = escapedText.Split(' ');
            foreach (var word in words)
            {
                if (!string.IsNullOrWhiteSpace(word))
                {
                    Stats.WordCount++;
                }
            }
            
            foreach (var letter in text)
            {
                if (!string.IsNullOrWhiteSpace(letter.ToString()) && letter != '.' && letter != '?' && letter != '!')
                {
                    Stats.CharacterCount ++;

                }
            }
            //old implementation
            //var escapedText = text.Replace("\r\n", "");
            //var sentences = escapedText.Split(new char[] { '.', '?', '!' });
            //foreach (var sentence in sentences)
            //{
            //    if (!string.IsNullOrWhiteSpace(sentence))
            //    {
            //        Stats.SentenceCount = Stats.SentenceCount + 1;
            //        var wordsInSentence = sentence.Split(' ');
            //        foreach (var word in wordsInSentence)
            //        {
            //            if (!string.IsNullOrWhiteSpace(word))
            //            {
            //                Stats.WordCount = Stats.WordCount + 1;
            //                foreach (var letter in word)
            //                {
            //                    if (!string.IsNullOrWhiteSpace(letter.ToString()))
            //                    {
            //                        Stats.CharacterCount = Stats.CharacterCount + 1;
            //                    }
            //                }
            //            }

            //        }
            //    }

            //}

            s.Stop();
            Stats.Duration = s.Elapsed;
        }

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    public class MultithreadedTextAnalyzer : ITextAnalyzer, INotifyPropertyChanged
    {
        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

        #region Properties
        public TextStatistics stats;
        public TextStatistics Stats
        {
            get
            {
                return stats;
            }
            set
            {
                stats = value;
                OnPropertyChanged("Stats");
            }
        }

        #endregion

        #region Constructor
        public MultithreadedTextAnalyzer()
        {
            Stats = new TextStatistics();
        }
        #endregion

        #region Locks
        private readonly object sentenceCtrLock = new object();
        private readonly object wordCtrLock = new object();
        private readonly object characterCtrLock = new object();

        #endregion

        public void AnalyzeText(string text)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            //ThreadPool.SetMinThreads(2, 2);

            Parallel.Invoke(
                // count number of sentences
                () =>
                {
                    var sentences = text.Split(new char[] { '.', '?', '!' });
                    var tempCtr = 0;

                    Parallel.ForEach(sentences, sentence =>
                    {
                        if (!string.IsNullOrWhiteSpace(sentence))
                        {
                            // microsoft's thread-safe way of modifying a resource being accesed by multiple threads
                            Interlocked.Increment(ref tempCtr);
                        }
                    });
                    Stats.SentenceCount = tempCtr;
                },
                // at the same time, count number of words
                () =>
                {
                    var escapedText = text.Replace("\r\n", "");
                    var words = escapedText.Split(' ');
                    var tempCtr = 0;
                    Parallel.ForEach(words, word =>
                    {
                        if (!string.IsNullOrWhiteSpace(word))
                        {
                            // microsoft's thread-safe way of modifying a resource being accesed by multiple threads
                            Interlocked.Increment(ref tempCtr);
                        }
                    });
                    Stats.WordCount = tempCtr;
                },
                 // at the same time, count number of letters in the entire wall of text
                 () =>
                 {
                     var tempCtr = 0;
                     Parallel.ForEach(text, letter =>
                     {
                         if (!string.IsNullOrWhiteSpace(letter.ToString()) && letter != '.' && letter != '?' && letter != '!')
                         {
                             // microsoft's thread-safe way of modifying a resource being accesed by multiple threads
                             Interlocked.Increment(ref tempCtr);
                         }
                     });
                     Stats.CharacterCount = tempCtr;
                 }
                );

            // old implementation... keeping it here for reference
            //Parallel.ForEach(text.Split(new char[] { '.', '?', '!' }),
            //sentence =>
            //{
            //    if (!string.IsNullOrWhiteSpace(sentence))
            //    {
            //        Stats.SentenceCount = Stats.SentenceCount + 1;
            //        Parallel.ForEach(sentence.Split(' '),
            //        word =>
            //        {
            //            if (!string.IsNullOrWhiteSpace(word))
            //            {
            //                Stats.WordCount = Stats.WordCount + 1;
            //                Parallel.ForEach(word,
            //                letter =>
            //                {
            //                    if (!string.IsNullOrWhiteSpace(letter.ToString()))
            //                    {
            //                        Stats.CharacterCount = Stats.CharacterCount + 1;
            //                    }
            //                });
            //            }
            //        });
            //    }
            //});
            s.Stop();
            Stats.Duration = s.Elapsed;
        }


    }

}
