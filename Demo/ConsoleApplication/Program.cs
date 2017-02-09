using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using PanGu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication
{
    class Program
    {
        static DirectoryInfo INDEX_DIR = new DirectoryInfo("index");
        static PanGuAnalyzer ANALYZER = new PanGuAnalyzer(); //MMSegAnalyzer //StandardAnalyzer

        static void Main(string[] args)
        {
            while (true)
            {
                ANALYZER.Options = new PanGu.Match.MatchOptions()
                {
                    ChineseNameIdentify = true,
                    TraditionalChineseEnabled = true,
                    OutputSimplifiedTraditional = true
                };
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
                    Console.WriteLine(content.Length > 50 ? content.Substring(0, 50) + "..." : "");
                }
            }
        }

        private static void CreateIndex(string[] contents)
        {
            var writer = new IndexWriter(FSDirectory.Open(INDEX_DIR), ANALYZER, true,
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
