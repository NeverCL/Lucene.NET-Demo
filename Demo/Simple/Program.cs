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
        static Analyzer analyzer; //MMSegAnalyzer //StandardAnalyzer
        private static string KEY = "我要去北京天安门广场,今天去了发现真大啊";
        static void Main(string[] args)
        {

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
