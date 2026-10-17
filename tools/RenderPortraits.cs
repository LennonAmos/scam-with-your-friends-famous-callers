using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

// Original vector-style fan portraits. No external artwork or fonts are bundled.
internal static class RenderPortraits
{
    static Graphics g;
    static Color C(string hex) { return ColorTranslator.FromHtml(hex); }
    static void E(string color, float x, float y, float w, float h, bool outline = true)
    { using (var b = new SolidBrush(C(color))) g.FillEllipse(b,x,y,w,h); if(outline) using(var p=new Pen(C("#202738"),4)) g.DrawEllipse(p,x,y,w,h); }
    static void R(string color, float x, float y, float w, float h, bool outline = true)
    { using(var b=new SolidBrush(C(color))) g.FillRectangle(b,x,y,w,h); if(outline) using(var p=new Pen(C("#202738"),4)) g.DrawRectangle(p,x,y,w,h); }
    static void P(string color, params float[] v)
    { var points=new PointF[v.Length/2]; for(int i=0;i<points.Length;i++) points[i]=new PointF(v[i*2],v[i*2+1]); using(var b=new SolidBrush(C(color))) g.FillPolygon(b,points); using(var p=new Pen(C("#202738"),4){LineJoin=LineJoin.Round}) g.DrawPolygon(p,points); }
    static void L(string color, float width, params float[] v)
    { var points=new PointF[v.Length/2]; for(int i=0;i<points.Length;i++) points[i]=new PointF(v[i*2],v[i*2+1]); using(var p=new Pen(C(color),width){StartCap=LineCap.Round,EndCap=LineCap.Round,LineJoin=LineJoin.Round}) g.DrawLines(p,points); }
    static void Eyes(float x=192,float y=215,float distance=94,float w=55,float h=58)
    { E("#fffdf7",x,y,w,h); E("#fffdf7",x+distance,y,w,h); E("#202738",x+w*.48f,y+h*.4f,10,14,false); E("#202738",x+distance+w*.35f,y+h*.4f,10,14,false); }
    static void Bust(string coat,string shirt="#f8f3df",string tie=null)
    { E(coat,70,360,372,300); P(shirt,211,377,301,377,319,512,193,512); if(tie!=null){P(tie,236,393,276,393,267,422,282,505,256,527,230,505,245,422);} }
    static void Human(string skin,string hair,bool glasses=false)
    { E(skin,145,207,44,62); E(skin,323,207,44,62); E(skin,163,112,186,286); if(hair!=null){P(hair,167,206,163,145,190,103,251,84,312,101,347,147,344,196,322,144,270,140,213,157,185,151);} Eyes(190,211,80,49,49); L("#202738",4,250,246,239,288,263,290); L("#202738",4,222,330,258,340,289,326); if(glasses){R("#ffffff",181,205,70,58,false);R("#ffffff",262,205,70,58,false);Eyes(190,212,80,49,45); using(var p=new Pen(C("#202738"),6)){g.DrawRectangle(p,181,205,70,58);g.DrawRectangle(p,262,205,70,58);g.DrawLine(p,251,222,262,222);}} }
    static void Draw(string id)
    {
        switch(id)
        {
        case "homer_simpson":
            Bust("#fffdf7"); E("#f8d73a",145,185,215,252); E("#f8d73a",307,250,47,58); E("#f8d73a",159,113,176,245); Eyes(168,211,76,70,70); E("#d6a865",174,286,166,111); E("#f8d73a",239,260,92,45); L("#202738",5,211,346,259,354,305,336); L("#202738",4,219,129,212,100,226,84,243,94); L("#202738",4,248,123,249,95,266,87,279,111); P("#ffffff",178,404,234,443,180,469,155,422); P("#ffffff",277,434,325,405,348,441,303,466); break;
        case "marge_simpson":
            Bust("#88bc67"); E("#326bba",166,15,182,321); for(int k=0;k<7;k++){E("#326bba",158+(k%2)*9,29+k*32,197,62);} E("#f8d73a",174,256,159,176); Eyes(183,286,66,56,57); E("#f8d73a",237,333,88,31); L("#202738",4,204,382,244,389,289,373); for(int k=0;k<8;k++) E("#db5546",178+k*20,418+(float)Math.Sin(k*.45)*9,23,23); break;
        case "montgomery_burns":
            Bust("#267d66","#f7efe0","#663c63"); E("#f1cd76",180,100,142,288); E("#d9e4de",156,188,53,99); E("#d9e4de",303,186,51,103); Eyes(174,202,83,67,56); P("#f1cd76",259,234,268,286,330,300,287,321,250,297); L("#202738",6,177,190,234,201);L("#202738",6,270,201,325,188);L("#202738",4,220,346,266,351,297,333);L("#aa9158",3,217,164,285,166); break;
        case "peter_griffin":
            Bust("#faf7e9"); E("#ecc5a1",122,138,264,284); E("#ecc5a1",198,337,117,88); P("#744b31",133,212,124,155,157,113,214,91,283,96,340,124,373,177,343,187,320,149,274,139,208,152,171,177); Eyes(154,219,100,77,65); using(var p=new Pen(C("#202738"),5)){g.DrawEllipse(p,145,213,94,77);g.DrawEllipse(p,249,213,94,77);g.DrawLine(p,238,241,250,241);} E("#ecc5a1",225,265,98,40); L("#202738",4,182,322,226,337,304,329); L("#202738",4,238,378,256,391,275,378); break;
        case "stan_smith":
            Bust("#243b67","#fdf8e8","#b82d41"); P("#e2b18d",156,149,208,105,302,105,355,165,338,328,292,399,215,399,166,329); P("#233047",151,199,155,133,191,82,278,74,335,106,361,166,338,207,318,143,210,153,175,190); Eyes(188,210,77,52,48);L("#202738",8,181,199,235,202);L("#202738",8,266,202,321,198);P("#e2b18d",252,240,268,287,239,286);L("#202738",5,209,329,304,329);L("#b38a70",4,223,367,289,367);break;
        case "roger":
            E("#a6acb7",111,345,290,280); E("#a6acb7",122,58,268,338); E("#a6acb7",199,252,113,182); E("#262938",159,171,74,120); E("#262938",278,171,74,120); E("#ffffff",176,190,14,23,false); E("#ffffff",291,190,14,23,false); L("#404553",5,222,348,268,355,298,338); P("#b8769e",187,402,251,427,328,396,315,458,257,446,205,464); break;
        case "philip_j_fry":
            Bust("#bf4338"); E("#ecc6a2",159,164,190,244);P("#ec7833",150,235,140,143,186,153,157,92,218,114,225,53,274,104,322,66,323,135,366,123,348,208,320,161,219,181,172,211);Eyes(183,223,86,58,58);E("#ecc6a2",251,268,77,35);L("#202738",4,209,337,260,350,299,328);P("#a93730",180,373,209,421,187,473,153,404);break;
        case "turanga_leela":
            Bust("#f4f2e9");E("#8b4fb2",265,114,144,267);E("#e8bca4",162,151,186,250);P("#8b4fb2",151,258,148,156,173,101,248,83,329,120,356,189,343,243,309,167,257,177,207,154,175,233);E("#fffdf7",190,216,131,68);E("#202738",244,236,24,32,false);L("#202738",4,250,279,241,304,267,306);L("#202738",4,219,348,280,348);P("#e8bca4",211,387,301,387,302,436,211,436);break;
        case "bender":
            E("#87919a",111,361,290,260);R("#87919a",164,147,184,257);E("#aeb9bf",164,112,184,73);R("#87919a",245,76,23,54);E("#bcc9cb",239,56,35,35);R("#39424a",151,209,210,77);Eyes(177,218,87,70,57);R("#e9e6bc",183,314,147,63);for(int k=1;k<5;k++) L("#202738",3,183+k*29,315,183+k*29,377);L("#202738",3,183,345,330,345);R("#626e78",183,421,147,93);break;
        case "rick_sanchez":
            Bust("#f1f1e8","#91c5c0");P("#a4d7e7",155,292,108,241,148,218,96,176,151,165,125,103,184,120,189,56,233,95,266,44,289,102,343,64,336,131,400,119,366,179,408,208,359,246,365,298,324,282);E("#e3c3a5",170,152,176,247);Eyes(186,219,79,57,61);L("#90c2d1",12,182,206,239,209);L("#90c2d1",12,267,209,322,204);L("#202738",4,248,257,240,303,266,304);L("#202738",4,213,340,295,340);L("#8ac8a1",9,253,344,256,365,266,360);break;
        case "spongebob":
            R("#e5ad7a",159,387,194,121);R("#f8da39",116,105,280,296);for(int k=0;k<10;k++)E("#c5b13c",130+(k*69)%240,120+(k*53)%242,18+(k%3)*3,24,false);Eyes(162,193,98,85,93);E("#58a9d3",194,225,30,39);E("#58a9d3",288,225,30,39);E("#202738",202,236,13,20,false);E("#202738",297,236,13,20,false);E("#f8da39",233,253,47,80);L("#202738",5,155,331,194,357,300,357,353,328);R("#fffdf7",224,355,27,35);R("#fffdf7",255,355,27,35);R("#ffffff",118,400,276,37);P("#bc3e39",241,403,272,403,264,428,277,461,256,477,237,459,248,428);R("#84512d",151,457,67,14);R("#84512d",297,457,67,14);break;
        case "patrick_star":
            P("#ec9c9c",89,433,155,302,216,133,256,67,296,133,357,302,423,433,346,417,327,510,184,510,164,417);Eyes(204,239,52,52,75);L("#202738",5,194,344,248,367,310,345);R("#a4cf6c",163,439,188,74);P("#8a60ad",182,451,205,446,215,465,199,484,177,478);P("#8a60ad",283,472,303,455,328,470,319,495,291,497);E("#bd787e",251,403,12,12,false);break;
        case "squidward_tentacles":
            Bust("#a77b46","#a77b46");E("#8ac6b4",143,83,226,226);R("#8ac6b4",194,263,125,145);Eyes(185,195,64,67,92);R("#a7cbbb",186,196,132,34,false);E("#75b3a3",230,235,64,143);L("#202738",5,204,384,308,384);E("#bd8b6f",204,235,18,29,false);E("#bd8b6f",274,235,18,29,false);L("#5c9e8e",3,204,139,309,139);L("#5c9e8e",3,211,154,300,154);break;
        case "mr_krabs":
            E("#cf4e46",109,236,295,207);P("#9ac6dd",128,373,385,373,412,512,100,512);L("#cc4c43",26,201,261,189,154);L("#cc4c43",26,310,261,322,154);E("#f4ecb7",165,97,52,112);E("#f4ecb7",295,97,52,112);E("#202738",183,123,14,33,false);E("#202738",314,123,14,33,false);E("#cf4e46",61,311,92,108);E("#cf4e46",361,311,92,108);L("#202738",4,91,315,113,360,136,334);L("#202738",4,378,334,397,360,422,315);L("#202738",5,182,322,254,345,327,322);R("#523d43",108,469,294,26);R("#d5af54",236,463,48,39);break;
        case "shrek":
            Bust("#735c3c","#e7d9b8");E("#97b958",151,121,211,285);L("#97b958",29,172,196,117,143);E("#97b958",89,113,57,62);L("#97b958",29,340,196,395,143);E("#97b958",369,113,57,62);Eyes(189,214,73,55,49);L("#456333",8,181,204,237,211);L("#456333",8,268,211,326,202);E("#8aab4e",218,265,87,48);L("#202738",5,202,331,250,351,311,331);break;
        case "donkey":
            E("#8e969d",134,358,245,257);E("#8e969d",136,62,64,215);E("#b8b2b4",151,86,30,160);E("#8e969d",309,62,64,215);E("#b8b2b4",325,86,30,160);E("#8e969d",160,198,194,213);P("#363947",207,217,211,170,240,196,254,152,271,195,294,175,294,226);Eyes(181,239,84,62,69);E("#c6c0ba",147,304,218,126);E("#363947",188,323,25,21,false);E("#363947",300,323,25,21,false);L("#202738",5,183,380,253,402,329,379);R("#fffdf7",234,382,39,24);break;
        case "walter_white":
            Bust("#8b835c","#e6e1c9");Human("#d9b392",null,true);P("#605143",213,317,232,337,278,337,302,313,291,378,255,392,222,377);P("#d9b392",237,341,274,341,274,363,238,363);L("#605143",8,223,310,251,301,284,311);R("#272a30",167,111,177,48);P("#272a30",188,109,199,54,314,54,326,109);R("#20232a",138,151,237,22);break;
        case "saul_goodman":
            Bust("#48789a","#b7d6e6","#e85c51");Human("#d5a480","#684a34");P("#fff8e9",218,325,290,322,277,348,231,350);L("#7d5544",3,210,303,222,318);L("#7d5544",3,295,301,288,319);L("#202738",6,181,197,231,190);L("#202738",6,273,191,319,201);break;
        case "michael_scott":
            Bust("#303e55","#f1f0e9","#803e51");Human("#dfb89d","#3b302d");L("#202738",7,182,199,232,192);L("#202738",7,272,192,321,203);P("#fff8ed",220,325,288,325,278,344,233,344);L("#aa8a77",3,204,302,216,321);L("#aa8a77",3,301,302,292,321);break;
        case "dwight_schrute":
            Bust("#d6b44e","#d6b44e","#695039");Human("#dfbd9d","#7c5e3d",true);P("#7c5e3d",177,161,194,113,245,95,283,111,334,142,327,175,280,144,250,126,214,155);L("#202738",6,180,193,237,195);L("#202738",6,269,195,325,192);L("#202738",5,217,338,291,338);break;
        default: throw new ArgumentException("Unknown portrait: "+id);
        }
    }
    public static void Main(string[] args)
    {
        if(args.Length!=1) throw new ArgumentException("Usage: RenderPortraits.exe <portrait output folder>");
        Directory.CreateDirectory(args[0]);
        string[] ids={"homer_simpson","marge_simpson","montgomery_burns","peter_griffin","stan_smith","roger","philip_j_fry","turanga_leela","bender","rick_sanchez","spongebob","patrick_star","squidward_tentacles","mr_krabs","shrek","donkey","walter_white","saul_goodman","michael_scott","dwight_schrute"};
        foreach(string id in ids) using(var bitmap=new Bitmap(512,512,PixelFormat.Format32bppArgb))
        { using(g=Graphics.FromImage(bitmap)){g.SmoothingMode=SmoothingMode.AntiAlias;g.Clear(Color.Transparent);Draw(id);} bitmap.Save(Path.Combine(args[0],id+".png"),ImageFormat.Png); }
        Console.WriteLine("Rendered "+ids.Length+" original fan portraits.");
    }
}
