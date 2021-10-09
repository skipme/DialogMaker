// -----------------------------------------------------------------------
// <copyright file="Backlog.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace DialogMaker
{
    using System.Collections.Generic;
    using System.Drawing;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class DBacklog : IDialog
    {
        IDialog baseDialog;
        int max;
        int currentcommand = -1;

        public List<DCommand> commands = new List<DCommand>();
        public void ClearHistory()
        {
            commands.Clear();
            currentcommand = -1;
            System.GC.Collect();
        }
        public DBacklog(IDialog dialog, int maxcommands = 25)
        {
            baseDialog = dialog;
            max = maxcommands;
        }

        public enum PHCommand
        {
            emtycommand,
            PhraseAdded = 0x10,
            PhraseMove,
            PhraseMoveTree,
            PhraseRemove,
            TimelineAdd,
            TimelineMove,
            TimelineRemove,

            setLabel,
            setDescription,

            PhraseConnectionLost,
            PhraseConnected
        }


        void Push(DCommand c)
        {
            int cursor = currentcommand;

            if (cursor >= 0)
            {
                if ((c.ComType == PHCommand.PhraseMove || c.ComType == PHCommand.PhraseMoveTree) &&
                    commands[cursor].ComType == c.ComType)
                {
                    if ((int)c.EngagedObject1 == (int)commands[cursor].EngagedObject1)
                    {
                        Point nwpt = (Point)commands[cursor].EngagedObject2;
                        nwpt.Offset((Point)c.EngagedObject2);
                        commands[cursor].EngagedObject2 = nwpt;
                        return;
                    }
                }
            }


            if ((currentcommand + 1) >= 0 && (currentcommand + 1) < commands.Count)
                commands[currentcommand + 1] = c;
            else
                commands.Add(c);
            currentcommand = ++cursor;//commands.Count - 1;
            if (commands.Count >= max + 10)
            {
                commands.RemoveRange(0, 10);
                currentcommand -= 10;
            }
            //

            //Console.WriteLine("{0} :{1}", c.ComType, currentcommand);
        }

        public int AddPhrase(Phrase p)
        {
            int pindex = baseDialog.AddPhrase(p);
            Push(new DCommand() { ComType = PHCommand.PhraseAdded, EngagedObject1 = p, EngagedObject2 = pindex });
            return pindex;
        }

        public int AddPhrase(Phrase p, int connection)
        {
            int pindex = baseDialog.AddPhrase(p, connection);
            Push(new DCommand() { ComType = PHCommand.PhraseAdded, EngagedObject1 = p, EngagedObject2 = pindex });
            return pindex;
        }
        public void RemoveConnection(int phraseIndex, int referencePhrase)
        {
            Push(new DCommand() { ComType = PHCommand.PhraseConnectionLost, EngagedObject1 = phraseIndex, EngagedObject2 = referencePhrase });
            baseDialog.RemoveConnection(phraseIndex, referencePhrase);
        }
        public void Connect(int phraseIndex, int referencePhrase)
        {
            Push(new DCommand() { ComType = PHCommand.PhraseConnected, EngagedObject1 = phraseIndex, EngagedObject2 = referencePhrase });
            baseDialog.Connect(phraseIndex, referencePhrase);
        }
        public void AddTimeline(TimeLine tl)
        {
            baseDialog.AddTimeline(tl);
        }

        public void ClonePhrase(int Index)
        {
            baseDialog.ClonePhrase(Index);
        }

        public void ClonePhraseTree(int Index)
        {
            baseDialog.ClonePhraseTree(Index);
        }

        public void Draw(SvgNet.SvgGdi.IGraphics gr, float scale, int selectedEdit, int selectedEditTL, System.Drawing.Point Translate, bool Clips, int width)
        {
            baseDialog.Draw(gr, scale, selectedEdit, selectedEditTL, Translate, Clips, width);
        }

        public int GetSelectedPhrase(System.Drawing.Point location, float scale, System.Drawing.Point Translate)
        {
            return baseDialog.GetSelectedPhrase(location, scale, Translate);
        }

        public int GetSelectedTimeLine(System.Drawing.Point location, float scale, System.Drawing.Point Translate)
        {
            return baseDialog.GetSelectedTimeLine(location, scale, Translate);
        }

        public string Json
        {
            get { return baseDialog.Json; }
        }

        public void MovePhrase(int Index, System.Drawing.Point offset)
        {
            baseDialog.MovePhrase(Index, offset);
            Push(new DCommand() { ComType = PHCommand.PhraseMove, EngagedObject1 = Index, EngagedObject2 = offset });
        }

        public void MovePhraseTree(int Index, System.Drawing.Point offset)
        {
            baseDialog.MovePhraseTree(Index, offset);
            Push(new DCommand() { ComType = PHCommand.PhraseMoveTree, EngagedObject1 = Index, EngagedObject2 = offset });
        }

        public List<int> RemovePhrase(int Index)
        {
            Phrase x = baseDialog.Phrase(Index);
            List<int> indexes = baseDialog.RemovePhrase(Index);
            Push(new DCommand() { ComType = PHCommand.PhraseRemove, EngagedObject1 = x, EngagedObject2 = indexes, EngagedObject3 = Index });

            foreach (DCommand dc in commands)
            {
                if (dc.ComType == PHCommand.PhraseRemove)
                {
                    for (int i = 0; i < ((List<int>)dc.EngagedObject2).Count; i++)
                    {
                        if (((List<int>)dc.EngagedObject2)[i] == Index)
                        {
                            ((List<int>)dc.EngagedObject2).RemoveAt(i);
                            i--;
                        }
                        else
                            if (((List<int>)dc.EngagedObject2)[i] >= Index)
                        {
                            ((List<int>)dc.EngagedObject2)[i]--;
                        }
                    }
                }
            }

            return indexes;
        }

        public void RemoveTimeline(int Index)
        {
            TimeLine x = baseDialog.Timeline(Index);
            baseDialog.RemoveTimeline(Index);
            Push(new DCommand() { ComType = PHCommand.TimelineRemove, EngagedObject1 = x });
        }

        public System.Drawing.Rectangle VisibleRect
        {
            get { return baseDialog.VisibleRect; }
        }

        public void Clear()
        {
            baseDialog.Clear();
        }

        public void SetSelectedColorspace(int FocusedEditPhrase, ColorSpace cs)
        {
            baseDialog.SetSelectedColorspace(FocusedEditPhrase, cs);
        }
        public void SetSelectedAttrib(int FocusedEditPhrase, int attribIndex, byte val)
        {
            baseDialog.SetSelectedAttrib(FocusedEditPhrase, attribIndex, val);
        }
        public void SetSelectedLabel(int FocusedEditPhrase, string text)
        {
            if (FocusedEditPhrase >= 0)
            {
                string prevText = baseDialog.Phrase(FocusedEditPhrase).Label;
                Push(new DCommand() { ComType = PHCommand.setLabel, EngagedObject1 = text, EngagedObject2 = prevText, EngagedObject3 = FocusedEditPhrase });
            }

            baseDialog.SetSelectedLabel(FocusedEditPhrase, text);
        }
        public void SetSelectedText(int FocusedEditTimeline, int FocusedEditPhrase, string text)
        {
            if (FocusedEditPhrase >= 0)
            {
                string prevText = baseDialog.Phrase(FocusedEditPhrase).Text;
                Push(new DCommand() { ComType = PHCommand.setDescription, EngagedObject1 = text, EngagedObject2 = prevText, EngagedObject3 = FocusedEditPhrase });
            }

            baseDialog.SetSelectedText(FocusedEditTimeline, FocusedEditPhrase, text);
        }

        public void SetSelectedImage(int FocusedEditPhrase, System.Drawing.Image image)
        {
            baseDialog.SetSelectedImage(FocusedEditPhrase, image);
        }

        public System.Drawing.Image GetSelectedImage(int FocusedEditPhrase)
        {
            return baseDialog.GetSelectedImage(FocusedEditPhrase);
        }

        public void RemoveSelectedImage(int FocusedEditPhrase)
        {
            baseDialog.RemoveSelectedImage(FocusedEditPhrase);
        }

        public Phrase Phrase(int Index)
        {
            return baseDialog.Phrase(Index);
        }

        public TimeLine Timeline(int Index)
        {
            return baseDialog.Timeline(Index);
        }

        public bool Undo()
        {
            if (currentcommand < 0 || commands.Count == 0)
                return false;

            bool doOk = commands[currentcommand].InvertCommand(baseDialog);
            currentcommand--;
            return doOk;
        }

        public void Redo()
        {
            //if (currentcommand < 0 || commands.Count == 0)
            //    return;

            //commands[currentcommand].InvertCommand(baseDialog);
            //currentcommand--;
        }

        public void DoNothing()
        {
            Push(new DCommand() { ComType = PHCommand.emtycommand });
        }


        public List<int> GetConnectedFrom(int phrase)
        {
            return baseDialog.GetConnectedFrom(phrase);
        }


        public string CopyJsonTree(int index)
        {
            return baseDialog.CopyJsonTree(index);
        }
        public void ParseJsonTree(string json)
        {
            baseDialog.ParseJsonTree(json);
        }


        public List<int> GetConnectedTo(int phrase)
        {
            return baseDialog.GetConnectedTo(phrase);
        }


        public List<int> GetPhrasesWithAttribute(int attributeIndex, byte attributeValue)
        {
            return baseDialog.GetPhrasesWithAttribute(attributeIndex, attributeValue);
        }

        public void SetFont(Font fontRef)
        {
            baseDialog.SetFont(fontRef);
        }
        public IEnumerable<(int, Phrase)> SearchPhrase(string contains, bool left = true, bool right = true)
        {
            return baseDialog.SearchPhrase(contains, left, right);
        }
        public void AdjustPhrasePositionsToGrid(int gridSizeX, int gridSizeY)
        {
            baseDialog.AdjustPhrasePositionsToGrid(gridSizeX, gridSizeY);
        }
    }
}
