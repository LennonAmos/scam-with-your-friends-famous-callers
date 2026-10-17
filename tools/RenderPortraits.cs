using System;using System.Drawing;using System.Drawing.Drawing2D;using System.Drawing.Imaging;using System.IO;
// Editable floating-head fan art, aligned with the game's native expression layers.
internal static class RenderPortraits
{
 static Graphics g; static Color C(string c){return ColorTranslator.FromHtml(c);}
 static void E(string c,float x,float y,float w,float h,bool ink=true){using(var b=new SolidBrush(C(c)))g.FillEllipse(b,x,y,w,h);if(ink)using(var p=new Pen(Color.Black,18))g.DrawEllipse(p,x,y,w,h);}
 static void R(string c,float x,float y,float w,float h,bool ink=true){using(var b=new SolidBrush(C(c)))g.FillRectangle(b,x,y,w,h);if(ink)using(var p=new Pen(Color.Black,18){LineJoin=LineJoin.Round})g.DrawRectangle(p,x,y,w,h);}
 static void P(string c,params float[] v){var a=new PointF[v.Length/2];for(int i=0;i<a.Length;i++)a[i]=new PointF(v[2*i],v[2*i+1]);using(var b=new SolidBrush(C(c)))g.FillPolygon(b,a);using(var p=new Pen(Color.Black,18){LineJoin=LineJoin.Round})g.DrawPolygon(p,a);}
 static void L(string c,float w,params float[] v){var a=new PointF[v.Length/2];for(int i=0;i<a.Length;i++)a[i]=new PointF(v[2*i],v[2*i+1]);using(var p=new Pen(C(c),w){StartCap=LineCap.Round,EndCap=LineCap.Round,LineJoin=LineJoin.Round})g.DrawLines(p,a);}
 static void Head(string skin,string shade){E(skin,65,112,387,335);E(shade,85,178,62,199,false);E(skin,113,158,322,245,false);}
 static void Nose(string skin){E(skin,262,253,69,47);}
 static void Hair(string c){P(c,70,225,60,147,90,90,160,54,241,53,307,73,365,62,420,115,453,184,426,232,388,158,338,147,304,121,252,151,209,123,155,153,115,157);}
 static void Wrinkles(){L("#b39b72",7,214,140,258,149,312,142);L("#b39b72",6,224,163,272,168,309,162);}
 static void Glasses(){L("#000000",12,139,211,244,211,240,257,157,257,139,211);L("#000000",12,283,211,392,211,375,257,288,257,283,211);L("#000000",10,244,221,283,221);}
 static void Draw(string id){switch(id){
 case "homer_simpson":Head("#efcf42","#d8b63b");E("#c5a56d",148,285,238,135);Nose("#efcf42");L("#000000",9,181,123,170,96,189,79,207,96);L("#000000",9,216,113,216,86,235,78,250,96);break;
 case "marge_simpson":Head("#efcf42","#d1b239");for(int n=5;n>=0;n--)E(n%2==0?"#3b76b2":"#356da6",149,15+n*18,214,55);Nose("#efcf42");E("#d4b63a",372,343,29,29,false);break;
 case "montgomery_burns":Head("#e6cc8e","#bca56e");E("#a9b8b5",46,186,82,113);E("#a9b8b5",398,183,70,111);P("#e6cc8e",267,237,280,285,369,302,298,326,250,288);Wrinkles();L("#b39b72",7,145,340,174,365);L("#b39b72",7,366,335,345,364);break;
 case "peter_griffin":Head("#e2c39b","#b59c7c");Hair("#705233");E("#e2c39b",188,387,134,62);Nose("#e2c39b");Glasses();L("#9e856a",8,240,409,257,423,279,407);break;
 case "stan_smith":P("#ddbb9b",71,157,138,95,352,94,443,171,414,352,355,436,168,436,94,355);Hair("#202637");P("#ddbb9b",270,249,304,296,262,292);L("#b29275",7,204,395,327,395);break;
 case "roger":E("#a6a6ae",93,64,327,372);E("#bfc0c5",111,102,69,178,false);L("#72717b",8,358,335,330,373);break;
 case "philip_j_fry":Head("#e4bd98","#ba987d");P("#c77d3b",62,229,60,138,111,143,84,86,156,110,166,48,228,93,277,47,291,113,356,65,350,134,423,121,432,220,383,155,302,165,245,138,183,164,119,167);Nose("#e4bd98");break;
 case "turanga_leela":E("#78518f",363,148,93,267);Head("#dbb6a0","#af8d80");P("#78518f",65,224,58,151,87,94,152,62,231,61,283,94,339,66,414,108,447,182,423,241,373,152,312,144,261,174,208,140,142,170,107,218);E("#f8f2df",169,207,205,80);E("#000000",255,225,30,43,false);L("#000000",10,217,352,282,361,328,348);break;
 case "bender":R("#929a9d",100,120,308,300);E("#b3b9ba",100,75,308,100);R("#9ba1a3",243,32,25,59);E("#aeb5b6",230,13,51,41);R("#282e30",91,206,324,89);E("#e9e5bc",120,217,125,67);E("#e9e5bc",263,217,125,67);E("#000000",184,240,21,26,false);E("#000000",299,240,21,26,false);R("#ded5a8",144,328,224,68);for(int i=1;i<6;i++)L("#000000",7,144+i*37,329,144+i*37,396);L("#000000",7,145,363,368,363);break;
 case "rick_sanchez":P("#a0c4c8",74,321,31,264,77,234,29,172,90,168,61,99,126,119,146,51,206,90,251,35,288,100,359,53,359,128,433,106,415,175,476,209,429,254,461,316,396,333);Head("#dfc49d","#b39c7a");Wrinkles();Nose("#dfc49d");L("#94bf9b",9,258,377,256,397,270,391);break;
 case "spongebob":P("#dfc95b",69,107,427,95,445,434,68,441,59,290);for(int i=0;i<9;i++)E("#b2a344",83+(i*57)%318,126+(i*63)%281,20,25,false);Nose("#dfc95b");R("#f5edcf",239,371,28,35);R("#f5edcf",276,371,28,35);break;
 case "patrick_star":P("#d59a99",59,423,111,288,184,105,256,48,327,108,396,292,457,423,344,442,177,442);E("#bc8085",101,327,52,70,false);L("#a97276",7,342,341,364,366);break;
 case "squidward_tentacles":Head("#95b6a7","#6c9284");E("#95b6a7",89,64,333,186);E("#8aab9b",258,246,81,142);L("#658b7d",7,186,132,327,132);L("#658b7d",6,200,151,317,151);break;
 case "mr_krabs":E("#b95d53",68,197,381,240);L("#a34c46",29,173,243,159,105);L("#a34c46",29,345,243,365,105);E("#d9d6a7",128,57,59,125);E("#d9d6a7",334,57,59,125);E("#000000",151,88,19,37,false);E("#000000",354,88,19,37,false);L("#000000",12,169,342,254,359,344,337);P("#000000",239,278,185,294,248,289,265,281,287,290,351,296,294,276);break;
 case "shrek":L("#8fa557",29,113,199,60,130);E("#7f944d",26,90,69,73);L("#8fa557",29,406,199,453,134);E("#7f944d",421,94,67,73);Head("#8fa557","#6d813d");Nose("#8fa557");E("#6f8342",126,336,38,49,false);break;
 case "donkey":E("#8c9595",98,42,77,215);E("#afa7a4",115,70,34,143,false);E("#8c9595",349,41,77,218);E("#afa7a4",371,67,31,143,false);Head("#8c9595","#6b7777");P("#34393b",182,144,181,101,223,126,246,83,274,128,313,105,319,151);E("#bcb4a7",127,295,270,145);E("#34393b",181,310,31,23,false);E("#34393b",315,310,31,23,false);break;
 case "walter_white":Head("#d5bd9b","#ab9277");P("#6c5a45",188,313,220,337,294,337,337,311,317,422,255,444,202,418);P("#d5bd9b",226,363,296,363,292,396,234,396);Glasses();Nose("#d5bd9b");R("#323232",116,111,286,55);P("#323232",148,110,170,48,347,48,369,110);R("#292a2b",84,156,352,19);break;
 case "saul_goodman":Head("#dbbc97","#ad9575");Hair("#735739");Nose("#dbbc97");L("#a58a6b",6,143,316,166,339);L("#a58a6b",6,387,314,365,337);break;
 case "michael_scott":Head("#dec1a0","#b1997e");Hair("#383736");Nose("#dec1a0");L("#b1997e",6,142,318,164,342);L("#b1997e",6,385,316,363,340);break;
 case "dwight_schrute":Head("#ddc49f","#ad9879");Hair("#8e734b");P("#8e734b",111,151,183,87,263,91,320,131,263,113,209,149,158,139);Glasses();Nose("#ddc49f");break;
 default:throw new ArgumentException(id);
 }}
 public static void Main(string[] args){if(args.Length!=1)throw new ArgumentException("Usage: RenderPortraits.exe <output folder>");Directory.CreateDirectory(args[0]);string[] ids={"homer_simpson","marge_simpson","montgomery_burns","peter_griffin","stan_smith","roger","philip_j_fry","turanga_leela","bender","rick_sanchez","spongebob","patrick_star","squidward_tentacles","mr_krabs","shrek","donkey","walter_white","saul_goodman","michael_scott","dwight_schrute"};foreach(string id in ids)using(var image=new Bitmap(737,734,PixelFormat.Format32bppArgb)){using(g=Graphics.FromImage(image)){g.SmoothingMode=SmoothingMode.AntiAlias;g.Clear(Color.Transparent);g.ScaleTransform(737f/512f,734f/512f);Draw(id);}image.Save(Path.Combine(args[0],id+".png"),ImageFormat.Png);}Console.WriteLine("Rendered 20 floating-head sprites aligned to native expression layers.");}
}
