using Godot;

namespace Underworld
{
	public partial class uimanager : Node2D
	{
		[ExportGroup("Messagescroll")]
		[Export] public RichTextLabel messageScrollUW1;
		[Export] public RichTextLabel messageScrollUW2;

		[Export] public TextureRect scrollEdgeUW1Left;
		[Export] public TextureRect scrollEdgeUW1Right;

		[Export] public TextureRect scrollEdgeUW2Left;
		[Export] public TextureRect scrollEdgeUW2Right;

		[Export] public TextureRect[] ConvoscrollEdgeUW1Left = new TextureRect[3];
		[Export] public TextureRect[] ConvoscrollEdgeUW1Right = new TextureRect[3];
		[Export] public TextureRect[] ConvoscrollEdgeUW2Left = new TextureRect[3];
		[Export] public TextureRect[] ConvoscrollEdgeUW2Right = new TextureRect[3];

		static int MessageScrollEdgeIndex = 0;
		static int ConvoScrollEdgeIndex = 0;

		public TextureRect scrollEdgeLeft
		{
			get
			{
				if (UWClass._RES == UWClass.GAME_UW2)
				{
					return scrollEdgeUW2Left;
				}
				else
				{
					return scrollEdgeUW1Left;
				}
			}
		}

		public TextureRect scrollEdgeRight
		{
			get
			{
				if (UWClass._RES == UWClass.GAME_UW2)
				{
					return scrollEdgeUW2Right;
				}
				else
				{
					return scrollEdgeUW1Right;
				}
			}
		}

		public TextureRect ConvoscrollEdgeLeft(int index)
		{
			if (UWClass._RES == UWClass.GAME_UW2)
			{
				return ConvoscrollEdgeUW2Left[index];
			}
			else
			{
				return ConvoscrollEdgeUW1Left[index];
			}
		}

		public TextureRect ConvoscrollEdgeRight(int index)
		{
			if (UWClass._RES == UWClass.GAME_UW2)
			{
				return ConvoscrollEdgeUW2Right[index];
			}
			else
			{
				return ConvoscrollEdgeUW1Right[index];
			}
		}

		/// <summary>
		/// Any typed content, eg "other text" in conversations, quantities to be picked up.
		/// Will auto replace the special text {TYPEDINPUT} in the scrolls.
		/// </summary>

		[Export] public LineEdit TypedInput;

		public MessageDisplay scroll = new();

		public MessageDisplay convo = new();

		public static bool MessageScrollIsTemporary = false;

		public static bool CursorOverMessageScroll = false; //Used to determine where cursor is when clicking conversation options

		/// <summary>
		/// forces text to appear in front of the next string outputed. Used to display messages like The sign says
		/// </summary>
		public static string NextOutputPrependedString = "";

		public static void InitMessageScrolls()
		{
			instance.scroll = new();
			for (int i = 0; i <= instance.scroll.Lines.GetUpperBound(0); i++)
			{
				instance.scroll.Lines[i] = new();
			}
			instance.scroll.OutputControl = new RichTextLabel[] { MessageScroll };

			instance.convo = new();
			if (UWClass._RES == UWClass.GAME_UW2)
			{
				instance.convo.Lines = new MessageScrollLine[12];
				instance.convo.Rows = 12;
				instance.convo.Columns = 40;
				instance.scroll.Columns = 44;
			}
			else
			{
				instance.convo.Lines = new MessageScrollLine[13];
				instance.convo.Rows = 13;
				instance.convo.Columns = 36;
			}

			for (int i = 0; i <= instance.convo.Lines.GetUpperBound(0); i++)
			{
				instance.convo.Lines[i] = new();
			}
			instance.convo.OutputControl = new RichTextLabel[] { ConversationText };

			//scrolledges
			instance.scrollEdgeLeft.Texture = grScrlEdge.LoadImageAt(0);
			instance.scrollEdgeRight.Texture = grScrlEdge.LoadImageAt(5);
			for (int i = 0; i < 3; i++)
			{
				instance.ConvoscrollEdgeLeft(i).Texture = grScrlEdge.LoadImageAt(10);
				instance.ConvoscrollEdgeLeft(i).Texture = grScrlEdge.LoadImageAt(16);
			}
		}

		public static RichTextLabel MessageScroll
		{
			get
			{
				switch (UWClass._RES)
				{
					case UWClass.GAME_UW2:
						return instance.messageScrollUW2;
					default:
						return instance.messageScrollUW1;
				}
			}
		}

		/// <summary>
		/// Adds to the bottom scroll
		/// </summary>
		/// <param name="stringToAdd"></param>
		/// <param name="option"></param>
		/// <param name="colour"></param>
		public static void AddToMessageScroll(string stringToAdd, int option = 0, int colour = 0, MessageDisplay.MessageDisplayMode mode = MessageDisplay.MessageDisplayMode.NormalMode)
		{
			if (MessageScrollIsTemporary)
			{//don't allow any overwriting when a message is already being displayed temporarily.
				return;
			}

			if (NextOutputPrependedString != "")
			{//forces messages to include text like "the sign says"
				stringToAdd = NextOutputPrependedString + stringToAdd;
				NextOutputPrependedString = "";
			}

			MessageScrollEdgeIndex = (MessageScrollEdgeIndex + 1) % 5;
			instance.scrollEdgeLeft.Texture = grScrlEdge.LoadImageAt(MessageScrollEdgeIndex);
			instance.scrollEdgeRight.Texture = grScrlEdge.LoadImageAt(MessageScrollEdgeIndex + 5);

			switch (mode)
			{
				case MessageDisplay.MessageDisplayMode.TemporaryMessage:
					{
						//back up lines
						var backup = BackupLines(instance.scroll.Lines, 5);
						MessageScrollIsTemporary = true;
						instance.scroll.Clear();
						_ = Peaky.Coroutines.Coroutine.Run(
							instance.scroll.AddText(newText: stringToAdd, option: option, colour: colour),
							main.instance
							);

						_ = Peaky.Coroutines.Coroutine.Run(
							instance.scroll.RestoreLinesAfterWait(backup, 2f),
							main.instance
							);
						break;
					}
				case MessageDisplay.MessageDisplayMode.NormalMode:
				case MessageDisplay.MessageDisplayMode.TypedInput:
				default:
					{
						_ = Peaky.Coroutines.Coroutine.Run(
								instance.scroll.AddText(newText: stringToAdd, option: option, colour: colour),
								main.instance
								);
						break;
					}
			}
			if (UWClass._RES != UWClass.GAME_UW2)
			{
				if (instance.scroll.LinePtr >= 5)
				{
					//in Uw1 after 5 lines of text are filled the dragons can start animating the message bar
					uimanager.StartDragonAnimation(1);
				}
			}
		}

		public static MessageScrollLine[] BackupLines(MessageScrollLine[] toBackup, int size)
		{
			var existingLines = new MessageScrollLine[size];
			for (int i = 0; i <= existingLines.GetUpperBound(0); i++)
			{
				existingLines[i] = new MessageScrollLine(toBackup[i].OptionNo, toBackup[i].LineText);
			}
			return existingLines;
		}


		/// <summary>
		/// Adds the text to the conversation scroll
		/// </summary>
		/// <param name="stringToAdd"></param>
		/// <param name="colour"></param>
		public static void AddToConvoScroll(string stringToAdd, int colour = 0)
		{
			ConvoScrollEdgeIndex = (ConvoScrollEdgeIndex + 1) % 6;
			for (int i = 0; i < 3; i++)
			{
				instance.ConvoscrollEdgeLeft(i).Texture = grScrlEdge.LoadImageAt(10 + ((ConvoScrollEdgeIndex + i) % 6));
				instance.ConvoscrollEdgeRight(i).Texture = grScrlEdge.LoadImageAt(16 + ((ConvoScrollEdgeIndex + i) % 6));
			}

			//MessageScroll.Text = stringToAdd;
			_ = Peaky.Coroutines.Coroutine.Run(
				instance.convo.AddText(newText: stringToAdd, colour: colour),
				main.instance
				);
		}

		/// <summary>
		/// Sets CursorOverMessageScroll to true when cursor enters the message scroll panel
		/// </summary>
		public void _on_message_scroll_mouse_entered()
		{
			//GD.Print("Cursor on scroll");
			CursorOverMessageScroll = true;
		}

		/// <summary>
		/// Sets CursorOverMessageScroll to false when cursor exits the message scroll panel
		/// </summary>
		public void _on_message_scroll_mouse_exit()
		{
			//GD.Print("Cursor exited scroll");
			CursorOverMessageScroll = false;
		}

		/// <summary>
		/// When presented with a list of options in the message scroll, this determines which option is clicked.
		/// </summary>
		/// <param name="mouseButton">Mouse click event if clicked in scroll message panel during conversation</param>
		public int HandleMessageScrollClick(InputEventMouseButton mouseButton)
		{
			//Calculate the height of a line in the rich text box
			int lineHeight = (int)MessageScroll.Size.Y / MessageScroll.GetLineCount();

			//Get the clicked position within the message scroll panel
			Vector2 localClickPosition = mouseButton.Position - MessageScroll.Position;

			//Calculate the clicked line
			int clickedLine = (int)(localClickPosition.Y / lineHeight);

			// Validate the clicked line index
			if (clickedLine >= 0 && clickedLine < MessageScroll.GetLineCount())
			{
				GD.Print($"Clicked line: {clickedLine + 1}");
				return clickedLine + 1;

			}
			else
			{
				GD.Print("Click was outside of valid lines.");
				return -1;
			}
		}

	} //end class

}//end namespace
