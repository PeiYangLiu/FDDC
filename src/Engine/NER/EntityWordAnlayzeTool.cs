using FDDC;
using JiebaNet.Analyser;
using JiebaNet.Segmenter;
using JiebaNet.Segmenter.PosSeg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
public static class EntityWordAnlayzeTool
{

    public const string 地名 = "ns";
    public const string 副词 = "d";
    public const string 了 = "ul";

    static PosSegmenter posSeg = new PosSegmenter();
    //甲方乙方首单词统计
    static Dictionary<String, int> FirstWordPos = new Dictionary<String, int>();
    static Dictionary<int, int> WordLength = new Dictionary<int, int>();

    public static void Init()
    {
        FirstWordPos.Clear();
        WordLength.Clear();
    }

    public static void PutFirstAndLengthWord(string Word)
    {
        if (String.IsNullOrEmpty(Word)) return;
        var words = posSeg.Cut(Word);
        if (words.Count() > 0)
        {
            var pos = words.First().Flag;
            if (FirstWordPos.ContainsKey(pos))
            {
                FirstWordPos[pos] = FirstWordPos[pos] + 1;
            }
            else
            {
                FirstWordPos.Add(pos, 1);
            }
            var wl = Word.Length;
            if (WordLength.ContainsKey(wl))
            {
                WordLength[wl] = WordLength[wl] + 1;
            }
            else
            {
                WordLength.Add(wl, 1);
            }
        }
    }

    public static void WriteFirstAndLengthWordToLog()
    {
        Program.Training.WriteLine("首词词性统计：");
        FindTop(5, FirstWordPos);
        Program.Training.WriteLine("词长统计：");
        FindTop(5, WordLength);
    }

    //寻找Top5
    static void FindTop<T>(int n, Dictionary<T, int> dict)
    {
        var Rank = dict.Values.ToList();
        Rank.Sort();
        Rank.Reverse();
        float Total = Rank.Sum();
        var pos = Math.Min(Rank.Count, n);
        int limit = Rank[n - 1];
        foreach (var key in dict.Keys)
        {
            if (dict[key] >= limit)
            {
                var percent = (dict[key] * 100 / Total) + "%";
                Program.Training.WriteLine(key + "(" + percent + ")");
            }
        }
    }

    public static void AnlayzeEntitySurroundWords(HTMLEngine.MyRootHtmlNode root, string KeyWord)
    {
        Program.Training.WriteLine("关键字：[" + KeyWord + "]");
        JiebaSegmenter segmenter = new JiebaSegmenter();
        segmenter.AddWord(KeyWord);
        foreach (var paragrah in root.Children)
        {
            var segments = segmenter.Cut(paragrah.FullText.NormalizeTextResult()).ToList();  // 默认为精确模式
            //Console.WriteLine("【精确模式】：{0}", string.Join("/ ", segments));
            //寻找关键字的位置
            for (int i = 0; i < segments.Count; i++)
            {
                if (segments[i].Equals(KeyWord))
                {
                    //前5个词语和后五个词语
                    var startInx = Math.Max(0, i - 5);
                    var EndInx = Math.Min(i + 5, segments.Count);
                    for (int s = startInx; s < i; s++)
                    {
                        Program.Training.WriteLine("前导关键字：[" + segments[s] + "]");
                        if (segments[s] == "：")
                        {
                            var leading = "";
                            for (int l = startInx; l < s; l++)
                            {
                                leading += segments[l];
                            }
                            Console.WriteLine("冒号前导词：" + leading);
                        }
                    }
                    Program.Training.WriteLine("关键字：[" + KeyWord + "]");
                    for (int s = i + 1; s < EndInx; s++)
                    {
                        Program.Training.WriteLine("后续关键字：[" + segments[s] + "]");
                    }

                }
            }
        }
    }

    public static string GetMainWordSentence(string OrgString)
    {
        var MainWordSentence = "";
        var pos = new JiebaNet.Segmenter.PosSeg.PosSegmenter();
        var list = pos.Cut(OrgString);
        foreach (var word in list)
        {
            //去除“副词”和“了”之后的句子
            if (word.Flag != EntityWordAnlayzeTool.了 &&
                word.Flag != EntityWordAnlayzeTool.副词)
            {
                MainWordSentence += word.Word;
            }
        }
        return MainWordSentence;
    }

    public static string TrimEnglish(string OrgString){
        var MainWordSentence = "";
        var pos = new JiebaNet.Segmenter.PosSeg.PosSegmenter();
        var list = pos.Cut(OrgString);
        foreach (var word in list)
        {
            //去除“副词”和“了”之后的句子
            if (word.Flag != "eng")
            {
                MainWordSentence += word.Word;
            }
        }
        return MainWordSentence;
    }

}