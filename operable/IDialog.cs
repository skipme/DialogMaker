using System;
using System.Collections.Generic;
namespace DialogMaker
{
    public interface IDialog
    {
        void DoNothing();// separate serias of commands
        int AddPhrase(global::DialogMaker.Phrase p);
        int AddPhrase(global::DialogMaker.Phrase p, int connection);
        void AddTimeline(global::DialogMaker.TimeLine tl);
        void ClonePhrase(int Index);
        void ClonePhraseTree(int Index);
        void Draw(global::SvgNet.SvgGdi.IGraphics gr, float scale, int selectedEdit, int selectedEditTL, global::System.Drawing.Point Translate, bool Clips, int width);
        int GetSelectedPhrase(global::System.Drawing.Point location, float scale, global::System.Drawing.Point Translate);
        int GetSelectedTimeLine(global::System.Drawing.Point location, float scale, global::System.Drawing.Point Translate);
        string Json { get; }
        void MovePhrase(int Index, global::System.Drawing.Point offset);
        void MovePhraseTree(int Index, global::System.Drawing.Point offset);
        global::System.Collections.Generic.List<int> RemovePhrase(int Index);
        void RemoveTimeline(int Index);
        void Clear();
        global::System.Drawing.Rectangle VisibleRect { get; }
        void SetSelectedColorspace(int FocusedEditPhrase, ColorSpace cs);
        void SetSelectedAttrib(int FocusedEditPhrase, int attribIndex, byte val);
        void SetSelectedText(int FocusedEditTimeline, int FocusedEditPhrase, string text);
        void SetSelectedImage(int FocusedEditPhrase, global::System.Drawing.Image image);
        global::System.Drawing.Image GetSelectedImage(int FocusedEditPhrase);
        void RemoveSelectedImage(int FocusedEditPhrase);
        void SetSelectedLabel(int FocusedEditPhrase, string text);
        Phrase Phrase(int Index);
        
        TimeLine Timeline(int Index);

        List<int> GetConnectedFrom(int phrase);
        List<int> GetConnectedTo(int phrase);
        List<int> GetPhrasesWithAttribute(int attributeIndex, byte attributeValue);

        void RemoveConnection(int phraseIndex, int referencePhrase);
        void Connect(int phraseIndex, int referencePhrase);

        string CopyJsonTree(int index);
        void ParseJsonTree(string json);

        void SetFont(System.Drawing.Font fontRef); 
        IEnumerable<(int, Phrase)> SearchPhrase(string contains, bool left = true, bool right = true);
        void AdjustPhrasePositionsToGrid(int gridSizeX, int gridSizeY);
    }
}
