using System;
using System.Diagnostics;
using Godot;

namespace Underworld
{
    public partial class uimanager : Node2D
    {

        [ExportGroup("Conversation")]
        //Conversation/Trade areas
        //UW1
        [Export] public Panel ConversationPanelUW1;
        [Export] public TextureRect PlayerPortraitUW1;
        [Export] public TextureRect NPCPortraitUW1;
        [Export] public Label PlayerNameLabelUW1;
        [Export] public Label NPCNameLabelUW1;
        [Export] public TextureRect PlayerPortraitFrameUW1;
        [Export] public TextureRect NPCPortraitFrameUW1;
        [Export] public TextureRect PlayerNameLableFrameUW1;
        [Export] public TextureRect NPCNameLableFrameUW1;
        [Export] public TextureRect PlayerTradeAreaUW1;
        [Export] public TextureRect NPCTradeAreaUW1;

        [Export] public TextureRect ConversationScrollTopUW1;
        [Export] public TextureRect ConversationScrollBottomUW1;

        [Export] public RichTextLabel ConversationTextUW1;

        //UW2 versions

        [Export] public Panel ConversationPanelUW2;
        [Export] public TextureRect PlayerPortraitUW2;
        [Export] public TextureRect NPCPortraitUW2;
        [Export] public Label PlayerNameLabelUW2;
        [Export] public Label NPCNameLabelUW2;
        [Export] public RichTextLabel ConversationTextUW2;

        //Conversation reference handlers to ensure the game appropiate ui element is always accessed.
        public static RichTextLabel ConversationText
        {
            get
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    return instance.ConversationTextUW2;
                }
                else
                {
                    return instance.ConversationTextUW1;
                }

            }
        }

        public TextureRect PlayerPortrait
        {
            get
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    return PlayerPortraitUW2;
                }
                else
                {
                    return PlayerPortraitUW1;
                }
            }
        }

        public TextureRect NPCPortrait
        {
            get
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    return NPCPortraitUW2;
                }
                else
                {
                    return NPCPortraitUW1;
                }
            }
        }

        public Label PlayerNameLabel
        {
            get
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    return PlayerNameLabelUW2;
                }
                else
                {
                    return PlayerNameLabelUW1;
                }
            }
        }

        public Label NPCNameLabel
        {
            get
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    return NPCNameLabelUW2;
                }
                else
                {
                    return NPCNameLabelUW1;
                }
            }
        }


        private void InitCoversation()
        {
            EnableDisable(ConversationPanelUW1,false);
            EnableDisable(ConversationPanelUW2,false);
            if (UWClass._RES != UWClass.GAME_UW2)
            {
                NPCNameLableFrameUW1.Texture = grConverse.LoadImageAt(0);
                PlayerNameLableFrameUW1.Texture = grConverse.LoadImageAt(0);
                PlayerTradeAreaUW1.Texture = grConverse.LoadImageAt(1);
                NPCTradeAreaUW1.Texture = grConverse.LoadImageAt(1);
                PlayerPortraitFrameUW1.Texture = grConverse.LoadImageAt(2);
                NPCPortraitFrameUW1.Texture = grConverse.LoadImageAt(2);
                ConversationScrollTopUW1.Texture = grConverse.LoadImageAt(3);
                ConversationScrollBottomUW1.Texture = grConverse.LoadImageAt(4);
            }
        }

    }//end class
}//end namespace