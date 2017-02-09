using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Tokenattributes;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using PanGu;
using Version = Lucene.Net.Util.Version;
using PanGu.HighLight;

namespace Simple
{
    class Program
    {
        static DirectoryInfo INDEX_DIR = new DirectoryInfo("index");
        static Analyzer analyzer = new PanGuAnalyzer(); //MMSegAnalyzer //StandardAnalyzer
        private static string KEY = "我要去北京天安门广场,今天去了发现真大啊";
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("输入文章:");
                CreateIndex(new[] { Console.ReadLine() });

                Console.WriteLine("输入关键字:");
                var words = new Segment().DoSegment(Console.ReadLine(), new PanGu.Match.MatchOptions()
                {
                    ChineseNameIdentify = true,
                    TraditionalChineseEnabled = true,
                    OutputSimplifiedTraditional = true
                });

                var phraseQuery = new PhraseQuery { Slop = 100 };           // 词组查询 
                foreach (var wordInfo in words)
                {
                    Console.WriteLine(wordInfo.Word);
                    phraseQuery.Add(new Term("body", wordInfo.Word));
                }

                var searcher = new IndexSearcher(FSDirectory.Open(INDEX_DIR), true);
                var tds = searcher.Search(phraseQuery, 10);
                Console.WriteLine("TotalHits: " + tds.TotalHits);

                foreach (ScoreDoc sd in tds.ScoreDocs)
                {
                    Document doc = searcher.Doc(sd.Doc);
                    var content = doc.Get("body");
                    Console.WriteLine(content.Substring(0, 50) + (content.Length > 50 ? "..." : ""));
                }
            }
            Console.ReadKey();
        }

        /// <summary>
        /// 高亮
        /// </summary>
        static void HighlighterWords()
        {
            var formatter = new SimpleHTMLFormatter("<font color=\"red\">", "</font>");
            //创建 Highlighter ，输入HTMLFormatter 和 盘古分词对象Semgent
            var highlighter = new Highlighter(formatter, new Segment());
            //设置每个摘要段的字符数
            highlighter.FragmentSize = 1000;
            //获取最匹配的摘要段
            var str = highlighter.GetBestFragment("北京天安门广场", KEY);
            Console.WriteLine(str);
        }

        /// <summary>
        /// 一元分词
        /// </summary>
        /// <param name="key"></param>
        private static void SegmentWordsByStand(string key)
        {
            analyzer = new StandardAnalyzer(Version.LUCENE_30);
            var tokenStream = analyzer.TokenStream("body", new StringReader(key));
            while (tokenStream.IncrementToken())
            {
                var attr = tokenStream.GetAttributeImplsIterator(); // Term Impls
                var types = tokenStream.GetAttributeTypesIterator(); // Term Types 
                Console.WriteLine((attr.ToArray()[0] as ITermAttribute).Term);
            }
        }

        private static void SegmentWordsByPanGuAnalyzer(string key)
        {
            analyzer = new PanGuAnalyzer();
            var tokenStream = analyzer.TokenStream("body", new StringReader(key));
            while (tokenStream.IncrementToken())
            {
                var attr = tokenStream.GetAttributeImplsIterator(); // Term Impls
                var types = tokenStream.GetAttributeTypesIterator(); // Term Types 
                Console.WriteLine((attr.ToArray()[0] as ITermAttribute).Term);
            }
        }



        /// <summary>
        /// 盘古分词
        /// </summary>
        /// <param name="key"></param>
        private static void SegmentWordsByPanGu(string key)
        {
            var words = new Segment().DoSegment(key);
            foreach (var wordInfo in words)
            {
                Console.WriteLine(wordInfo.Word);
            }
        }


        static void CreateIndex(string[] contents)
        {
            var writer = new IndexWriter(FSDirectory.Open(INDEX_DIR), analyzer, true,
                IndexWriter.MaxFieldLength.UNLIMITED);
            foreach (var content in contents)
            {
                var document = new Document();
                document.Add(new Field("body", content, Field.Store.YES, Field.Index.ANALYZED,
                    Field.TermVector.WITH_POSITIONS_OFFSETS));
                writer.AddDocument(document);
            }
            writer.Commit();
            writer.Optimize();
            writer.Dispose();
            Console.WriteLine("创建索引完成！");
        }
    }
}
