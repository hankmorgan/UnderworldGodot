using System;
using Godot;

namespace Underworld
{
    public partial class uimanager : Node2D
    {
        [ExportGroup("WeaponAnims")]
        [Export]
        public TextureRect weaponanimuw1;

        [Export]
        public TextureRect weaponanimuw2;

        static int CurrentWeaponFrame;
        static int PreviousWeaponAnimation;
        static double WeaponAnimTimer;

        static TextureRect WeaponAnim
        {
            get
            {
                if (UWClass._RES == UWClass.GAME_UW2)
                {
                    return instance.weaponanimuw2;
                }
                else
                {
                    return instance.weaponanimuw1;
                }
            }
        }

        public static int[,] weaponframes = new int[56, 6];

        public const int Sword_Slash_Right_Charge = 0;
        public const int Sword_Slash_Right_Execute = 1;
        public const int Sword_Bash_Right_Charge = 2;
        public const int Sword_Bash_Right_Execute = 3;
        public const int Sword_Stab_Right_Charge = 4;
        public const int Sword_Stab_Right_Execute = 5;
        public const int Sword_Ready_Right = 6;

        public const int Axe_Slash_Right_Charge = 7;
        public const int Axe_Slash_Right_Execute = 8;
        public const int Axe_Bash_Right_Charge = 9;
        public const int Axe_Bash_Right_Execute = 10;
        public const int Axe_Stab_Right_Charge = 11;
        public const int Axe_Stab_Right_Execute = 12;
        public const int Axe_Ready_Right = 13;

        public const int Mace_Slash_Right_Charge = 14;
        public const int Mace_Slash_Right_Execute = 15;
        public const int Mace_Bash_Right_Charge = 16;
        public const int Mace_Bash_Right_Execute = 17;
        public const int Mace_Stab_Right_Charge = 18;
        public const int Mace_Stab_Right_Execute = 19;
        public const int Mace_Ready_Right = 20;

        public const int Fist_Slash_Right_Charge = 21;
        public const int Fist_Slash_Right_Execute = 22;
        public const int Fist_Bash_Right_Charge = 23;
        public const int Fist_Bash_Right_Execute = 24;
        public const int Fist_Stab_Right_Charge = 25;
        public const int Fist_Stab_Right_Execute = 26;
        public const int Fist_Ready_Right = 27;

        public const int Sword_Slash_Left_Charge = 28;
        public const int Sword_Slash_Left_Execute = 29;
        public const int Sword_Bash_Left_Charge = 30;
        public const int Sword_Bash_Left_Execute = 31;
        public const int Sword_Stab_Left_Charge = 32;
        public const int Sword_Stab_Left_Execute = 33;
        public const int Sword_Ready_Left = 34;
        public const int Axe_Slash_Left_Charge = 35;
        public const int Axe_Slash_Left_Execute = 36;
        public const int Axe_Bash_Left_Charge = 37;
        public const int Axe_Bash_Left_Execute = 38;
        public const int Axe_Stab_Left_Charge = 39;
        public const int Axe_Stab_Left_Execute = 40;
        public const int Axe_Ready_Left = 41;
        public const int Mace_Slash_Left_Charge = 42;
        public const int Mace_Slash_Left_Execute = 43;
        public const int Mace_Bash_Left_Charge = 44;
        public const int Mace_Bash_Left_Execute = 45;
        public const int Mace_Stab_Left_Charge = 46;
        public const int Mace_Stab_Left_Execute = 47;
        public const int Mace_Ready_Left = 48;
        public const int Fist_Slash_Left_Charge = 49;
        public const int Fist_Slash_Left_Execute = 50;
        public const int Fist_Bash_Left_Charge = 51;
        public const int Fist_Bash_Left_Execute = 52;
        public const int Fist_Stab_Left_Charge = 53;
        public const int Fist_Stab_Left_Execute = 54;
        public const int Fist_Ready_Left = 55;

        public static int currentWeaponAnim;

        public static void InitWeaponAnimation()
        {
            //instance.weaponanimuw1.Texture = grWeapon.LoadImageAt(6);
            if (UWClass._RES == UWClass.GAME_UW2)
            {
                WeaponAnimationweaponframesUW2();
            }
            else
            {
                WeaponAnimationweaponframesUW1();
            }
        }

        private void _ProcessWeaponAnims(double delta)
        {
            if (InGame)
            {
                if (!main.blockmouseinput)
                {
                    WeaponAnimTimer += delta;
                    if (WeaponAnimTimer > 0.2f)
                    {
                        if (currentWeaponAnim != PreviousWeaponAnimation)
                        {
                            CurrentWeaponFrame = 0;
                        }
                        WeaponAnimTimer = 0;
                        if ((playerdat.play_drawn == 1) && (combat.isWeapon(playerdat.PrimaryHandObject) != 2))
                        {//weapon is drawn and is a melee weapon or fist                    
                            var frame = weaponframes[currentWeaponAnim, CurrentWeaponFrame];
                            if (frame != -1)
                            {
                                WeaponAnim.Texture = grWeapon.LoadImageAt(frame);
                            }
                            CurrentWeaponFrame = Math.Min(CurrentWeaponFrame + 1, 5);
                        }
                        else
                        {//weapon is put away or is a ranged weapon
                            WeaponAnim.Texture = null;
                        }
                        PreviousWeaponAnimation = currentWeaponAnim;
                    }
                }
            }
        }



        static void WeaponAnimationweaponframesUW1()
        {
            weaponframes[Sword_Slash_Right_Charge, 0] = 0; weaponframes[Sword_Slash_Right_Charge, 1] = 1;
            weaponframes[Sword_Slash_Right_Charge, 2] = 2; weaponframes[Sword_Slash_Right_Charge, 3] = 3;
            weaponframes[Sword_Slash_Right_Charge, 4] = -1; weaponframes[Sword_Slash_Right_Charge, 5] = -1;

            weaponframes[Sword_Slash_Right_Execute, 0] = 4; weaponframes[Sword_Slash_Right_Execute, 1] = 5;
            weaponframes[Sword_Slash_Right_Execute, 2] = 6; weaponframes[Sword_Slash_Right_Execute, 3] = 7;
            weaponframes[Sword_Slash_Right_Execute, 4] = 8; weaponframes[Sword_Slash_Right_Execute, 5] = 27;

            weaponframes[Sword_Bash_Right_Charge, 0] = 9; weaponframes[Sword_Bash_Right_Charge, 1] = 10;
            weaponframes[Sword_Bash_Right_Charge, 2] = 11; weaponframes[Sword_Bash_Right_Charge, 3] = 12;
            weaponframes[Sword_Bash_Right_Charge, 4] = -1; weaponframes[Sword_Bash_Right_Charge, 5] = -1;

            weaponframes[Sword_Bash_Right_Execute, 0] = 13; weaponframes[Sword_Bash_Right_Execute, 1] = 14;
            weaponframes[Sword_Bash_Right_Execute, 2] = 15; weaponframes[Sword_Bash_Right_Execute, 3] = 16;
            weaponframes[Sword_Bash_Right_Execute, 4] = 17; weaponframes[Sword_Bash_Right_Execute, 5] = 27;

            weaponframes[Sword_Stab_Right_Charge, 0] = 18; weaponframes[Sword_Stab_Right_Charge, 1] = 19;
            weaponframes[Sword_Stab_Right_Charge, 2] = 20; weaponframes[Sword_Stab_Right_Charge, 3] = 21;
            weaponframes[Sword_Stab_Right_Charge, 4] = -1; weaponframes[Sword_Stab_Right_Charge, 5] = -1;

            weaponframes[Sword_Stab_Right_Execute, 0] = 22; weaponframes[Sword_Stab_Right_Execute, 1] = 23;
            weaponframes[Sword_Stab_Right_Execute, 2] = 24; weaponframes[Sword_Stab_Right_Execute, 3] = 25;
            weaponframes[Sword_Stab_Right_Execute, 4] = 26; weaponframes[Sword_Stab_Right_Execute, 5] = 27;

            weaponframes[Sword_Ready_Right, 0] = 27; weaponframes[Sword_Ready_Right, 1] = -1;
            weaponframes[Sword_Ready_Right, 2] = -1; weaponframes[Sword_Ready_Right, 3] = -1;
            weaponframes[Sword_Ready_Right, 4] = -1; weaponframes[Sword_Ready_Right, 5] = -1;

            weaponframes[Axe_Slash_Right_Charge, 0] = 28; weaponframes[Axe_Slash_Right_Charge, 1] = 29;
            weaponframes[Axe_Slash_Right_Charge, 2] = 30; weaponframes[Axe_Slash_Right_Charge, 3] = 31;
            weaponframes[Axe_Slash_Right_Charge, 4] = -1; weaponframes[Axe_Slash_Right_Charge, 5] = -1;

            weaponframes[Axe_Slash_Right_Execute, 0] = 32; weaponframes[Axe_Slash_Right_Execute, 1] = 33;
            weaponframes[Axe_Slash_Right_Execute, 2] = 34; weaponframes[Axe_Slash_Right_Execute, 3] = 35;
            weaponframes[Axe_Slash_Right_Execute, 4] = 36; weaponframes[Axe_Slash_Right_Execute, 5] = 55;

            weaponframes[Axe_Bash_Right_Charge, 0] = 37; weaponframes[Axe_Bash_Right_Charge, 1] = 38;
            weaponframes[Axe_Bash_Right_Charge, 2] = 39; weaponframes[Axe_Bash_Right_Charge, 3] = 40;
            weaponframes[Axe_Bash_Right_Charge, 4] = -1; weaponframes[Axe_Bash_Right_Charge, 5] = -1;

            weaponframes[Axe_Bash_Right_Execute, 0] = 41; weaponframes[Axe_Bash_Right_Execute, 1] = 42;
            weaponframes[Axe_Bash_Right_Execute, 2] = 43; weaponframes[Axe_Bash_Right_Execute, 3] = 44;
            weaponframes[Axe_Bash_Right_Execute, 4] = 45; weaponframes[Axe_Bash_Right_Execute, 5] = 55;

            weaponframes[Axe_Stab_Right_Charge, 0] = 46; weaponframes[Axe_Stab_Right_Charge, 1] = 47;
            weaponframes[Axe_Stab_Right_Charge, 2] = 48; weaponframes[Axe_Stab_Right_Charge, 3] = 49;
            weaponframes[Axe_Stab_Right_Charge, 4] = -1; weaponframes[Axe_Stab_Right_Charge, 5] = -1;

            weaponframes[Axe_Stab_Right_Execute, 0] = 50; weaponframes[Axe_Stab_Right_Execute, 1] = 51;
            weaponframes[Axe_Stab_Right_Execute, 2] = 52; weaponframes[Axe_Stab_Right_Execute, 3] = 53;
            weaponframes[Axe_Stab_Right_Execute, 4] = 54; weaponframes[Axe_Stab_Right_Execute, 5] = 55;

            weaponframes[Axe_Ready_Right, 0] = 55; weaponframes[Axe_Ready_Right, 1] = -1;
            weaponframes[Axe_Ready_Right, 2] = -1; weaponframes[Axe_Ready_Right, 3] = -1;
            weaponframes[Axe_Ready_Right, 4] = -1; weaponframes[Axe_Ready_Right, 5] = -1;

            weaponframes[Mace_Slash_Right_Charge, 0] = 56; weaponframes[Mace_Slash_Right_Charge, 1] = 57;
            weaponframes[Mace_Slash_Right_Charge, 2] = 58; weaponframes[Mace_Slash_Right_Charge, 3] = 59;
            weaponframes[Mace_Slash_Right_Charge, 4] = -1; weaponframes[Mace_Slash_Right_Charge, 5] = -1;

            weaponframes[Mace_Slash_Right_Execute, 0] = 60; weaponframes[Mace_Slash_Right_Execute, 1] = 61;
            weaponframes[Mace_Slash_Right_Execute, 2] = 62; weaponframes[Mace_Slash_Right_Execute, 3] = 63;
            weaponframes[Mace_Slash_Right_Execute, 4] = 64; weaponframes[Mace_Slash_Right_Execute, 5] = 83;

            weaponframes[Mace_Bash_Right_Charge, 0] = 65; weaponframes[Mace_Bash_Right_Charge, 1] = 66;
            weaponframes[Mace_Bash_Right_Charge, 2] = 67; weaponframes[Mace_Bash_Right_Charge, 3] = 68;
            weaponframes[Mace_Bash_Right_Charge, 4] = -1; weaponframes[Mace_Bash_Right_Charge, 5] = -1;

            weaponframes[Mace_Bash_Right_Execute, 0] = 69; weaponframes[Mace_Bash_Right_Execute, 1] = 70;
            weaponframes[Mace_Bash_Right_Execute, 2] = 71; weaponframes[Mace_Bash_Right_Execute, 3] = 72;
            weaponframes[Mace_Bash_Right_Execute, 4] = 73; weaponframes[Mace_Bash_Right_Execute, 5] = 83;

            weaponframes[Mace_Stab_Right_Charge, 0] = 74; weaponframes[Mace_Stab_Right_Charge, 1] = 75;
            weaponframes[Mace_Stab_Right_Charge, 2] = 76; weaponframes[Mace_Stab_Right_Charge, 3] = 77;
            weaponframes[Mace_Stab_Right_Charge, 4] = -1; weaponframes[Mace_Stab_Right_Charge, 5] = -1;

            weaponframes[Mace_Stab_Right_Execute, 0] = 78; weaponframes[Mace_Stab_Right_Execute, 1] = 79;
            weaponframes[Mace_Stab_Right_Execute, 2] = 80; weaponframes[Mace_Stab_Right_Execute, 3] = 81;
            weaponframes[Mace_Stab_Right_Execute, 4] = 82; weaponframes[Mace_Stab_Right_Execute, 5] = 83;

            weaponframes[Mace_Ready_Right, 0] = 83; weaponframes[Mace_Ready_Right, 1] = -1;
            weaponframes[Mace_Ready_Right, 2] = -1; weaponframes[Mace_Ready_Right, 3] = -1;
            weaponframes[Mace_Ready_Right, 4] = -1; weaponframes[Mace_Ready_Right, 5] = -1;

            weaponframes[Fist_Slash_Right_Charge, 0] = 84; weaponframes[Fist_Slash_Right_Charge, 1] = 85;
            weaponframes[Fist_Slash_Right_Charge, 2] = 86; weaponframes[Fist_Slash_Right_Charge, 3] = 87;
            weaponframes[Fist_Slash_Right_Charge, 4] = -1; weaponframes[Fist_Slash_Right_Charge, 5] = -1;

            weaponframes[Fist_Slash_Right_Execute, 0] = 88; weaponframes[Fist_Slash_Right_Execute, 1] = 89;
            weaponframes[Fist_Slash_Right_Execute, 2] = 90; weaponframes[Fist_Slash_Right_Execute, 3] = 91;
            weaponframes[Fist_Slash_Right_Execute, 4] = 92; weaponframes[Fist_Slash_Right_Execute, 5] = 111;

            weaponframes[Fist_Bash_Right_Charge, 0] = 93; weaponframes[Fist_Bash_Right_Charge, 1] = 94;
            weaponframes[Fist_Bash_Right_Charge, 2] = 95; weaponframes[Fist_Bash_Right_Charge, 3] = 96;
            weaponframes[Fist_Bash_Right_Charge, 4] = -1; weaponframes[Fist_Bash_Right_Charge, 5] = 1;

            weaponframes[Fist_Bash_Right_Execute, 0] = 97; weaponframes[Fist_Bash_Right_Execute, 1] = 98;
            weaponframes[Fist_Bash_Right_Execute, 2] = 99; weaponframes[Fist_Bash_Right_Execute, 3] = 100;
            weaponframes[Fist_Bash_Right_Execute, 4] = 101; weaponframes[Fist_Bash_Right_Execute, 5] = 111;

            weaponframes[Fist_Stab_Right_Charge, 0] = 102; weaponframes[Fist_Stab_Right_Charge, 1] = 103;
            weaponframes[Fist_Stab_Right_Charge, 2] = 104; weaponframes[Fist_Stab_Right_Charge, 3] = 105;
            weaponframes[Fist_Stab_Right_Charge, 4] = -1; weaponframes[Fist_Stab_Right_Charge, 5] = -1;

            weaponframes[Fist_Stab_Right_Execute, 0] = 106; weaponframes[Fist_Stab_Right_Execute, 1] = 107;
            weaponframes[Fist_Stab_Right_Execute, 2] = 108; weaponframes[Fist_Stab_Right_Execute, 3] = 109;
            weaponframes[Fist_Stab_Right_Execute, 4] = 110; weaponframes[Fist_Stab_Right_Execute, 5] = 111;

            weaponframes[Fist_Ready_Right, 0] = 111; weaponframes[Fist_Ready_Right, 1] = -1;
            weaponframes[Fist_Ready_Right, 2] = -1; weaponframes[Fist_Ready_Right, 3] = -1;
            weaponframes[Fist_Ready_Right, 4] = -1; weaponframes[Fist_Ready_Right, 5] = -1;

            weaponframes[Sword_Slash_Left_Charge, 0] = 112; weaponframes[Sword_Slash_Left_Charge, 1] = 113;
            weaponframes[Sword_Slash_Left_Charge, 2] = 114; weaponframes[Sword_Slash_Left_Charge, 3] = 115;
            weaponframes[Sword_Slash_Left_Charge, 4] = -1; weaponframes[Sword_Slash_Left_Charge, 5] = -1;

            weaponframes[Sword_Slash_Left_Execute, 0] = 116; weaponframes[Sword_Slash_Left_Execute, 1] = 117;
            weaponframes[Sword_Slash_Left_Execute, 2] = 118; weaponframes[Sword_Slash_Left_Execute, 3] = 119;
            weaponframes[Sword_Slash_Left_Execute, 4] = 120; weaponframes[Sword_Slash_Left_Execute, 5] = 139;

            weaponframes[Sword_Bash_Left_Charge, 0] = 121; weaponframes[Sword_Bash_Left_Charge, 1] = 122;
            weaponframes[Sword_Bash_Left_Charge, 2] = 123; weaponframes[Sword_Bash_Left_Charge, 3] = 124;
            weaponframes[Sword_Bash_Left_Charge, 4] = -1; weaponframes[Sword_Bash_Left_Charge, 5] = -1;

            weaponframes[Sword_Bash_Left_Execute, 0] = 125; weaponframes[Sword_Bash_Left_Execute, 1] = 126;
            weaponframes[Sword_Bash_Left_Execute, 2] = 127; weaponframes[Sword_Bash_Left_Execute, 3] = 128;
            weaponframes[Sword_Bash_Left_Execute, 4] = 129; weaponframes[Sword_Bash_Left_Execute, 5] = 139;

            weaponframes[Sword_Stab_Left_Charge, 0] = 130; weaponframes[Sword_Stab_Left_Charge, 1] = 131;
            weaponframes[Sword_Stab_Left_Charge, 2] = 132; weaponframes[Sword_Stab_Left_Charge, 3] = 133;
            weaponframes[Sword_Stab_Left_Charge, 4] = -1; weaponframes[Sword_Stab_Left_Charge, 5] = -1;

            weaponframes[Sword_Stab_Left_Execute, 0] = 134; weaponframes[Sword_Stab_Left_Execute, 1] = 135;
            weaponframes[Sword_Stab_Left_Execute, 2] = 136; weaponframes[Sword_Stab_Left_Execute, 3] = 137;
            weaponframes[Sword_Stab_Left_Execute, 4] = 138; weaponframes[Sword_Stab_Left_Execute, 5] = 139;

            weaponframes[Sword_Ready_Left, 0] = 139; weaponframes[Sword_Ready_Left, 1] = -1;
            weaponframes[Sword_Ready_Left, 2] = -1; weaponframes[Sword_Ready_Left, 3] = -1;
            weaponframes[Sword_Ready_Left, 4] = -1; weaponframes[Sword_Ready_Left, 5] = -1;

            weaponframes[Axe_Slash_Left_Charge, 0] = 140; weaponframes[Axe_Slash_Left_Charge, 1] = 141;
            weaponframes[Axe_Slash_Left_Charge, 2] = 142; weaponframes[Axe_Slash_Left_Charge, 3] = 143;
            weaponframes[Axe_Slash_Left_Charge, 4] = -1; weaponframes[Axe_Slash_Left_Charge, 5] = -1;

            weaponframes[Axe_Slash_Left_Execute, 0] = 144; weaponframes[Axe_Slash_Left_Execute, 1] = 145;
            weaponframes[Axe_Slash_Left_Execute, 2] = 146; weaponframes[Axe_Slash_Left_Execute, 3] = 147;
            weaponframes[Axe_Slash_Left_Execute, 4] = 148; weaponframes[Axe_Slash_Left_Execute, 5] = 167;

            weaponframes[Axe_Bash_Left_Charge, 0] = 149; weaponframes[Axe_Bash_Left_Charge, 1] = 150;
            weaponframes[Axe_Bash_Left_Charge, 2] = 151; weaponframes[Axe_Bash_Left_Charge, 3] = 152;
            weaponframes[Axe_Bash_Left_Charge, 4] = -1; weaponframes[Axe_Bash_Left_Charge, 5] = -1;

            weaponframes[Axe_Bash_Left_Execute, 0] = 153; weaponframes[Axe_Bash_Left_Execute, 1] = 154;
            weaponframes[Axe_Bash_Left_Execute, 2] = 155; weaponframes[Axe_Bash_Left_Execute, 3] = 156;
            weaponframes[Axe_Bash_Left_Execute, 4] = 157; weaponframes[Axe_Bash_Left_Execute, 5] = 167;

            weaponframes[Axe_Stab_Left_Charge, 0] = 158; weaponframes[Axe_Stab_Left_Charge, 1] = 159;
            weaponframes[Axe_Stab_Left_Charge, 2] = 160; weaponframes[Axe_Stab_Left_Charge, 3] = 161;
            weaponframes[Axe_Stab_Left_Charge, 4] = -1; weaponframes[Axe_Stab_Left_Charge, 5] = -1;

            weaponframes[Axe_Stab_Left_Execute, 0] = 162; weaponframes[Axe_Stab_Left_Execute, 1] = 163;
            weaponframes[Axe_Stab_Left_Execute, 2] = 164; weaponframes[Axe_Stab_Left_Execute, 3] = 165;
            weaponframes[Axe_Stab_Left_Execute, 4] = 166; weaponframes[Axe_Stab_Left_Execute, 5] = 167;

            weaponframes[Axe_Ready_Left, 0] = 167; weaponframes[Axe_Ready_Left, 1] = -1;
            weaponframes[Axe_Ready_Left, 2] = -1; weaponframes[Axe_Ready_Left, 3] = -1;
            weaponframes[Axe_Ready_Left, 4] = -1; weaponframes[Axe_Ready_Left, 5] = -1;

            weaponframes[Mace_Slash_Left_Charge, 0] = 168; weaponframes[Mace_Slash_Left_Charge, 1] = 169;
            weaponframes[Mace_Slash_Left_Charge, 2] = 170; weaponframes[Mace_Slash_Left_Charge, 3] = 171;
            weaponframes[Mace_Slash_Left_Charge, 4] = -1; weaponframes[Mace_Slash_Left_Charge, 5] = -1;

            weaponframes[Mace_Slash_Left_Execute, 0] = 172; weaponframes[Mace_Slash_Left_Execute, 1] = 173;
            weaponframes[Mace_Slash_Left_Execute, 2] = 174; weaponframes[Mace_Slash_Left_Execute, 3] = 175;
            weaponframes[Mace_Slash_Left_Execute, 4] = 176; weaponframes[Mace_Slash_Left_Execute, 5] = 195;

            weaponframes[Mace_Bash_Left_Charge, 0] = 177; weaponframes[Mace_Bash_Left_Charge, 1] = 178;
            weaponframes[Mace_Bash_Left_Charge, 2] = 179; weaponframes[Mace_Bash_Left_Charge, 3] = 180;
            weaponframes[Mace_Bash_Left_Charge, 4] = -1; weaponframes[Mace_Bash_Left_Charge, 5] = -1;

            weaponframes[Mace_Bash_Left_Execute, 0] = 181; weaponframes[Mace_Bash_Left_Execute, 1] = 182;
            weaponframes[Mace_Bash_Left_Execute, 2] = 183; weaponframes[Mace_Bash_Left_Execute, 3] = 184;
            weaponframes[Mace_Bash_Left_Execute, 4] = 185; weaponframes[Mace_Bash_Left_Execute, 5] = 195;

            weaponframes[Mace_Stab_Left_Charge, 0] = 186; weaponframes[Mace_Stab_Left_Charge, 1] = 187;
            weaponframes[Mace_Stab_Left_Charge, 2] = 188; weaponframes[Mace_Stab_Left_Charge, 3] = 189;
            weaponframes[Mace_Stab_Left_Charge, 4] = -1; weaponframes[Mace_Stab_Left_Charge, 5] = -1;

            weaponframes[Mace_Stab_Left_Execute, 0] = 190; weaponframes[Mace_Stab_Left_Execute, 1] = 191;
            weaponframes[Mace_Stab_Left_Execute, 2] = 192; weaponframes[Mace_Stab_Left_Execute, 3] = 193;
            weaponframes[Mace_Stab_Left_Execute, 4] = 194; weaponframes[Mace_Stab_Left_Execute, 5] = 195;

            weaponframes[Mace_Ready_Left, 0] = 195; weaponframes[Mace_Ready_Left, 1] = -1;
            weaponframes[Mace_Ready_Left, 2] = -1; weaponframes[Mace_Ready_Left, 3] = -1;
            weaponframes[Mace_Ready_Left, 4] = -1; weaponframes[Mace_Ready_Left, 5] = -1;

            weaponframes[Fist_Slash_Left_Charge, 0] = 196; weaponframes[Fist_Slash_Left_Charge, 1] = 197;
            weaponframes[Fist_Slash_Left_Charge, 2] = 198; weaponframes[Fist_Slash_Left_Charge, 3] = 199;
            weaponframes[Fist_Slash_Left_Charge, 4] = -1; weaponframes[Fist_Slash_Left_Charge, 5] = -1;

            weaponframes[Fist_Slash_Left_Execute, 0] = 200; weaponframes[Fist_Slash_Left_Execute, 1] = 201;
            weaponframes[Fist_Slash_Left_Execute, 2] = 202; weaponframes[Fist_Slash_Left_Execute, 3] = 203;
            weaponframes[Fist_Slash_Left_Execute, 4] = 204; weaponframes[Fist_Slash_Left_Execute, 5] = 223;

            weaponframes[Fist_Bash_Left_Charge, 0] = 205; weaponframes[Fist_Bash_Left_Charge, 1] = 206;
            weaponframes[Fist_Bash_Left_Charge, 2] = 207; weaponframes[Fist_Bash_Left_Charge, 3] = 208;
            weaponframes[Fist_Bash_Left_Charge, 4] = -1; weaponframes[Fist_Bash_Left_Charge, 5] = -1;

            weaponframes[Fist_Bash_Left_Execute, 0] = 209; weaponframes[Fist_Bash_Left_Execute, 1] = 210;
            weaponframes[Fist_Bash_Left_Execute, 2] = 211; weaponframes[Fist_Bash_Left_Execute, 3] = 212;
            weaponframes[Fist_Bash_Left_Execute, 4] = 213; weaponframes[Fist_Bash_Left_Execute, 5] = 223;

            weaponframes[Fist_Stab_Left_Charge, 0] = 214; weaponframes[Fist_Stab_Left_Charge, 1] = 215;
            weaponframes[Fist_Stab_Left_Charge, 2] = 216; weaponframes[Fist_Stab_Left_Charge, 3] = 217;
            weaponframes[Fist_Stab_Left_Charge, 4] = -1; weaponframes[Fist_Stab_Left_Charge, 5] = -1;

            weaponframes[Fist_Stab_Left_Execute, 0] = 218; weaponframes[Fist_Stab_Left_Execute, 1] = 219;
            weaponframes[Fist_Stab_Left_Execute, 2] = 220; weaponframes[Fist_Stab_Left_Execute, 3] = 221;
            weaponframes[Fist_Stab_Left_Execute, 4] = 222; weaponframes[Fist_Stab_Left_Execute, 5] = 223;

            weaponframes[Fist_Ready_Left, 0] = 223; weaponframes[Fist_Ready_Left, 1] = -1;
            weaponframes[Fist_Ready_Left, 2] = -1; weaponframes[Fist_Ready_Left, 3] = -1;
            weaponframes[Fist_Ready_Left, 4] = -1; weaponframes[Fist_Ready_Left, 5] = -1;
        }

        static void WeaponAnimationweaponframesUW2()
        {
            weaponframes[Sword_Slash_Right_Charge, 0] = 0; weaponframes[Sword_Slash_Right_Charge, 1] = 1;
            weaponframes[Sword_Slash_Right_Charge, 2] = 2; weaponframes[Sword_Slash_Right_Charge, 3] = -1;
            weaponframes[Sword_Slash_Right_Charge, 4] = -1; weaponframes[Sword_Slash_Right_Charge, 5] = -1;

            weaponframes[Sword_Slash_Right_Execute, 0] = 4; weaponframes[Sword_Slash_Right_Execute, 1] = 5;
            weaponframes[Sword_Slash_Right_Execute, 2] = 6; weaponframes[Sword_Slash_Right_Execute, 3] = 7;
            weaponframes[Sword_Slash_Right_Execute, 4] = -1; weaponframes[Sword_Slash_Right_Execute, 5] = -1;

            weaponframes[Sword_Bash_Right_Charge, 0] = 9; weaponframes[Sword_Bash_Right_Charge, 1] = 10;
            weaponframes[Sword_Bash_Right_Charge, 2] = 11; weaponframes[Sword_Bash_Right_Charge, 3] = -1;
            weaponframes[Sword_Bash_Right_Charge, 4] = -1; weaponframes[Sword_Bash_Right_Charge, 5] = -1;

            weaponframes[Sword_Bash_Right_Execute, 0] = 13; weaponframes[Sword_Bash_Right_Execute, 1] = 14;
            weaponframes[Sword_Bash_Right_Execute, 2] = 15; weaponframes[Sword_Bash_Right_Execute, 3] = 16;
            weaponframes[Sword_Bash_Right_Execute, 4] = -1; weaponframes[Sword_Bash_Right_Execute, 5] = -1;

            weaponframes[Sword_Stab_Right_Charge, 0] = 27; weaponframes[Sword_Stab_Right_Charge, 1] = 28;
            weaponframes[Sword_Stab_Right_Charge, 2] = 29; weaponframes[Sword_Stab_Right_Charge, 3] = -1;
            weaponframes[Sword_Stab_Right_Charge, 4] = -1; weaponframes[Sword_Stab_Right_Charge, 5] = -1;

            weaponframes[Sword_Stab_Right_Execute, 0] = 22; weaponframes[Sword_Stab_Right_Execute, 1] = 23;
            weaponframes[Sword_Stab_Right_Execute, 2] = 24; weaponframes[Sword_Stab_Right_Execute, 3] = 24;
            weaponframes[Sword_Stab_Right_Execute, 4] = 23; weaponframes[Sword_Stab_Right_Execute, 5] = 22;

            weaponframes[Sword_Ready_Right, 0] = 27; weaponframes[Sword_Ready_Right, 1] = -1;
            weaponframes[Sword_Ready_Right, 2] = -1; weaponframes[Sword_Ready_Right, 3] = -1;
            weaponframes[Sword_Ready_Right, 4] = -1; weaponframes[Sword_Ready_Right, 5] = -1;

            weaponframes[Axe_Slash_Right_Charge, 0] = 31; weaponframes[Axe_Slash_Right_Charge, 1] = 32;
            weaponframes[Axe_Slash_Right_Charge, 2] = 33; weaponframes[Axe_Slash_Right_Charge, 3] = -1;
            weaponframes[Axe_Slash_Right_Charge, 4] = -1; weaponframes[Axe_Slash_Right_Charge, 5] = -1;

            weaponframes[Axe_Slash_Right_Execute, 0] = 35; weaponframes[Axe_Slash_Right_Execute, 1] = 36;
            weaponframes[Axe_Slash_Right_Execute, 2] = 37; weaponframes[Axe_Slash_Right_Execute, 3] = 38;
            weaponframes[Axe_Slash_Right_Execute, 4] = -1; weaponframes[Axe_Slash_Right_Execute, 5] = -1;

            weaponframes[Axe_Bash_Right_Charge, 0] = 40; weaponframes[Axe_Bash_Right_Charge, 1] = 41;
            weaponframes[Axe_Bash_Right_Charge, 2] = 42; weaponframes[Axe_Bash_Right_Charge, 3] = -1;
            weaponframes[Axe_Bash_Right_Charge, 4] = -1; weaponframes[Axe_Bash_Right_Charge, 5] = -1;

            weaponframes[Axe_Bash_Right_Execute, 0] = 44; weaponframes[Axe_Bash_Right_Execute, 1] = 45;
            weaponframes[Axe_Bash_Right_Execute, 2] = 46; weaponframes[Axe_Bash_Right_Execute, 3] = 47;
            weaponframes[Axe_Bash_Right_Execute, 4] = -1; weaponframes[Axe_Bash_Right_Execute, 5] = -1;

            weaponframes[Axe_Stab_Right_Charge, 0] = 58; weaponframes[Axe_Stab_Right_Charge, 1] = 59;
            weaponframes[Axe_Stab_Right_Charge, 2] = 60; weaponframes[Axe_Stab_Right_Charge, 3] = -1;
            weaponframes[Axe_Stab_Right_Charge, 4] = -1; weaponframes[Axe_Stab_Right_Charge, 5] = -1;

            weaponframes[Axe_Stab_Right_Execute, 0] = 53; weaponframes[Axe_Stab_Right_Execute, 1] = 54;
            weaponframes[Axe_Stab_Right_Execute, 2] = 55; weaponframes[Axe_Stab_Right_Execute, 3] = 55;
            weaponframes[Axe_Stab_Right_Execute, 4] = 54; weaponframes[Axe_Stab_Right_Execute, 5] = 53;

            weaponframes[Axe_Ready_Right, 0] = 58; weaponframes[Axe_Ready_Right, 1] = -1;
            weaponframes[Axe_Ready_Right, 2] = -1; weaponframes[Axe_Ready_Right, 3] = -1;
            weaponframes[Axe_Ready_Right, 4] = -1; weaponframes[Axe_Ready_Right, 5] = -1;

            weaponframes[Mace_Slash_Right_Charge, 0] = 62; weaponframes[Mace_Slash_Right_Charge, 1] = 63;
            weaponframes[Mace_Slash_Right_Charge, 2] = 64; weaponframes[Mace_Slash_Right_Charge, 3] = -1;
            weaponframes[Mace_Slash_Right_Charge, 4] = -1; weaponframes[Mace_Slash_Right_Charge, 5] = -1;

            weaponframes[Mace_Slash_Right_Execute, 0] = 66; weaponframes[Mace_Slash_Right_Execute, 1] = 67;
            weaponframes[Mace_Slash_Right_Execute, 2] = 68; weaponframes[Mace_Slash_Right_Execute, 3] = 69;
            weaponframes[Mace_Slash_Right_Execute, 4] = -1; weaponframes[Mace_Slash_Right_Execute, 5] = -1;

            weaponframes[Mace_Bash_Right_Charge, 0] = 71; weaponframes[Mace_Bash_Right_Charge, 1] = 72;
            weaponframes[Mace_Bash_Right_Charge, 2] = 73; weaponframes[Mace_Bash_Right_Charge, 3] = -1;
            weaponframes[Mace_Bash_Right_Charge, 4] = -1; weaponframes[Mace_Bash_Right_Charge, 5] = -1;

            weaponframes[Mace_Bash_Right_Execute, 0] = 75; weaponframes[Mace_Bash_Right_Execute, 1] = 76;
            weaponframes[Mace_Bash_Right_Execute, 2] = 77; weaponframes[Mace_Bash_Right_Execute, 3] = 78;
            weaponframes[Mace_Bash_Right_Execute, 4] = -1; weaponframes[Mace_Bash_Right_Execute, 5] = -1;

            weaponframes[Mace_Stab_Right_Charge, 0] = 89; weaponframes[Mace_Stab_Right_Charge, 1] = 90;
            weaponframes[Mace_Stab_Right_Charge, 2] = 91; weaponframes[Mace_Stab_Right_Charge, 3] = -1;
            weaponframes[Mace_Stab_Right_Charge, 4] = -1; weaponframes[Mace_Stab_Right_Charge, 5] = -1;

            weaponframes[Mace_Stab_Right_Execute, 0] = 84; weaponframes[Mace_Stab_Right_Execute, 1] = 85;
            weaponframes[Mace_Stab_Right_Execute, 2] = 86; weaponframes[Mace_Stab_Right_Execute, 3] = 86;
            weaponframes[Mace_Stab_Right_Execute, 4] = 85; weaponframes[Mace_Stab_Right_Execute, 5] = 84;

            weaponframes[Mace_Ready_Right, 0] = 89; weaponframes[Mace_Ready_Right, 1] = -1;
            weaponframes[Mace_Ready_Right, 2] = -1; weaponframes[Mace_Ready_Right, 3] = -1;
            weaponframes[Mace_Ready_Right, 4] = -1; weaponframes[Mace_Ready_Right, 5] = -1;

            weaponframes[Fist_Slash_Right_Charge, 0] = 102; weaponframes[Fist_Slash_Right_Charge, 1] = -1;
            weaponframes[Fist_Slash_Right_Charge, 2] = -1; weaponframes[Fist_Slash_Right_Charge, 3] = -1;
            weaponframes[Fist_Slash_Right_Charge, 4] = -1; weaponframes[Fist_Slash_Right_Charge, 5] = -1;

            weaponframes[Fist_Slash_Right_Execute, 0] = 97; weaponframes[Fist_Slash_Right_Execute, 1] = 98;
            weaponframes[Fist_Slash_Right_Execute, 2] = 99; weaponframes[Fist_Slash_Right_Execute, 3] = -1;
            weaponframes[Fist_Slash_Right_Execute, 4] = -1; weaponframes[Fist_Slash_Right_Execute, 5] = -1;

            weaponframes[Fist_Bash_Right_Charge, 0] = 102; weaponframes[Fist_Bash_Right_Charge, 1] = -1;
            weaponframes[Fist_Bash_Right_Charge, 2] = -1; weaponframes[Fist_Bash_Right_Charge, 3] = -1;
            weaponframes[Fist_Bash_Right_Charge, 4] = -1; weaponframes[Fist_Bash_Right_Charge, 5] = -1;

            weaponframes[Fist_Bash_Right_Execute, 0] = 97; weaponframes[Fist_Bash_Right_Execute, 1] = 98;
            weaponframes[Fist_Bash_Right_Execute, 2] = 99; weaponframes[Fist_Bash_Right_Execute, 3] = -1;
            weaponframes[Fist_Bash_Right_Execute, 4] = -1; weaponframes[Fist_Bash_Right_Execute, 5] = -1;

            weaponframes[Fist_Stab_Right_Charge, 0] = 102; weaponframes[Fist_Stab_Right_Charge, 1] = -1;
            weaponframes[Fist_Stab_Right_Charge, 2] = -1; weaponframes[Fist_Stab_Right_Charge, 3] = -1;
            weaponframes[Fist_Stab_Right_Charge, 4] = -1; weaponframes[Fist_Stab_Right_Charge, 5] = -1;

            weaponframes[Fist_Stab_Right_Execute, 0] = 97; weaponframes[Fist_Stab_Right_Execute, 1] = 98;
            weaponframes[Fist_Stab_Right_Execute, 2] = 99; weaponframes[Fist_Stab_Right_Execute, 3] = 99;
            weaponframes[Fist_Stab_Right_Execute, 4] = 98; weaponframes[Fist_Stab_Right_Execute, 5] = 97;

            weaponframes[Fist_Ready_Right, 0] = 102; weaponframes[Fist_Ready_Right, 1] = -1;
            weaponframes[Fist_Ready_Right, 2] = -1; weaponframes[Fist_Ready_Right, 3] = -1;
            weaponframes[Fist_Ready_Right, 4] = -1; weaponframes[Fist_Ready_Right, 5] = -1;

            weaponframes[Sword_Slash_Left_Charge, 0] = 124; weaponframes[Sword_Slash_Left_Charge, 1] = 125;
            weaponframes[Sword_Slash_Left_Charge, 2] = 126; weaponframes[Sword_Slash_Left_Charge, 3] = -1;
            weaponframes[Sword_Slash_Left_Charge, 4] = -1; weaponframes[Sword_Slash_Left_Charge, 5] = -1;

            weaponframes[Sword_Slash_Left_Execute, 0] = 128; weaponframes[Sword_Slash_Left_Execute, 1] = 129;
            weaponframes[Sword_Slash_Left_Execute, 2] = 130; weaponframes[Sword_Slash_Left_Execute, 3] = 131;
            weaponframes[Sword_Slash_Left_Execute, 4] = -1; weaponframes[Sword_Slash_Left_Execute, 5] = -1;

            weaponframes[Sword_Bash_Left_Charge, 0] = 133; weaponframes[Sword_Bash_Left_Charge, 1] = 134;
            weaponframes[Sword_Bash_Left_Charge, 2] = 135; weaponframes[Sword_Bash_Left_Charge, 3] = -1;
            weaponframes[Sword_Bash_Left_Charge, 4] = -1; weaponframes[Sword_Bash_Left_Charge, 5] = -1;

            weaponframes[Sword_Bash_Left_Execute, 0] = 137; weaponframes[Sword_Bash_Left_Execute, 1] = 138;
            weaponframes[Sword_Bash_Left_Execute, 2] = 139; weaponframes[Sword_Bash_Left_Execute, 3] = 140;
            weaponframes[Sword_Bash_Left_Execute, 4] = -1; weaponframes[Sword_Bash_Left_Execute, 5] = -1;

            weaponframes[Sword_Stab_Left_Charge, 0] = 152; weaponframes[Sword_Stab_Left_Charge, 1] = -1;
            weaponframes[Sword_Stab_Left_Charge, 2] = -1; weaponframes[Sword_Stab_Left_Charge, 3] = -1;
            weaponframes[Sword_Stab_Left_Charge, 4] = -1; weaponframes[Sword_Stab_Left_Charge, 5] = -1;

            weaponframes[Sword_Stab_Left_Execute, 0] = 146; weaponframes[Sword_Stab_Left_Execute, 1] = 147;
            weaponframes[Sword_Stab_Left_Execute, 2] = 148; weaponframes[Sword_Stab_Left_Execute, 3] = 148;
            weaponframes[Sword_Stab_Left_Execute, 4] = 147; weaponframes[Sword_Stab_Left_Execute, 5] = 146;

            weaponframes[Sword_Ready_Left, 0] = 152; weaponframes[Sword_Ready_Left, 1] = -1;
            weaponframes[Sword_Ready_Left, 2] = -1; weaponframes[Sword_Ready_Left, 3] = -1;
            weaponframes[Sword_Ready_Left, 4] = -1; weaponframes[Sword_Ready_Left, 5] = -1;

            weaponframes[Axe_Slash_Left_Charge, 0] = 154; weaponframes[Axe_Slash_Left_Charge, 1] = 155;
            weaponframes[Axe_Slash_Left_Charge, 2] = 156; weaponframes[Axe_Slash_Left_Charge, 3] = 157;
            weaponframes[Axe_Slash_Left_Charge, 4] = -1; weaponframes[Axe_Slash_Left_Charge, 5] = -1;

            weaponframes[Axe_Slash_Left_Execute, 0] = 159; weaponframes[Axe_Slash_Left_Execute, 1] = 160;
            weaponframes[Axe_Slash_Left_Execute, 2] = 161; weaponframes[Axe_Slash_Left_Execute, 3] = 162;
            weaponframes[Axe_Slash_Left_Execute, 4] = -1; weaponframes[Axe_Slash_Left_Execute, 5] = -1;

            weaponframes[Axe_Bash_Left_Charge, 0] = 164; weaponframes[Axe_Bash_Left_Charge, 1] = 165;
            weaponframes[Axe_Bash_Left_Charge, 2] = 166; weaponframes[Axe_Bash_Left_Charge, 3] = -1;
            weaponframes[Axe_Bash_Left_Charge, 4] = -1; weaponframes[Axe_Bash_Left_Charge, 5] = -1;

            weaponframes[Axe_Bash_Left_Execute, 0] = 168; weaponframes[Axe_Bash_Left_Execute, 1] = 169;
            weaponframes[Axe_Bash_Left_Execute, 2] = 170; weaponframes[Axe_Bash_Left_Execute, 3] = 171;
            weaponframes[Axe_Bash_Left_Execute, 4] = -1; weaponframes[Axe_Bash_Left_Execute, 5] = -1;

            weaponframes[Axe_Stab_Left_Charge, 0] = 183; weaponframes[Axe_Stab_Left_Charge, 1] = -1;
            weaponframes[Axe_Stab_Left_Charge, 2] = -1; weaponframes[Axe_Stab_Left_Charge, 3] = -1;
            weaponframes[Axe_Stab_Left_Charge, 4] = -1; weaponframes[Axe_Stab_Left_Charge, 5] = -1;

            weaponframes[Axe_Stab_Left_Execute, 0] = 177; weaponframes[Axe_Stab_Left_Execute, 1] = 178;
            weaponframes[Axe_Stab_Left_Execute, 2] = 179; weaponframes[Axe_Stab_Left_Execute, 3] = 179;
            weaponframes[Axe_Stab_Left_Execute, 4] = 178; weaponframes[Axe_Stab_Left_Execute, 5] = 177;

            weaponframes[Axe_Ready_Left, 0] = 182; weaponframes[Axe_Ready_Left, 1] = -1;
            weaponframes[Axe_Ready_Left, 2] = -1; weaponframes[Axe_Ready_Left, 3] = -1;
            weaponframes[Axe_Ready_Left, 4] = -1; weaponframes[Axe_Ready_Left, 5] = -1;

            weaponframes[Mace_Slash_Left_Charge, 0] = 186; weaponframes[Mace_Slash_Left_Charge, 1] = 187;
            weaponframes[Mace_Slash_Left_Charge, 2] = 188; weaponframes[Mace_Slash_Left_Charge, 3] = -1;
            weaponframes[Mace_Slash_Left_Charge, 4] = -1; weaponframes[Mace_Slash_Left_Charge, 5] = -1;

            weaponframes[Mace_Slash_Left_Execute, 0] = 190; weaponframes[Mace_Slash_Left_Execute, 1] = 191;
            weaponframes[Mace_Slash_Left_Execute, 2] = 192; weaponframes[Mace_Slash_Left_Execute, 3] = 193;
            weaponframes[Mace_Slash_Left_Execute, 4] = -1; weaponframes[Mace_Slash_Left_Execute, 5] = -1;

            weaponframes[Mace_Bash_Left_Charge, 0] = 195; weaponframes[Mace_Bash_Left_Charge, 1] = 196;
            weaponframes[Mace_Bash_Left_Charge, 2] = 197; weaponframes[Mace_Bash_Left_Charge, 3] = -1;
            weaponframes[Mace_Bash_Left_Charge, 4] = -1; weaponframes[Mace_Bash_Left_Charge, 5] = -1;

            weaponframes[Mace_Bash_Left_Execute, 0] = 199; weaponframes[Mace_Bash_Left_Execute, 1] = 200;
            weaponframes[Mace_Bash_Left_Execute, 2] = 201; weaponframes[Mace_Bash_Left_Execute, 3] = 202;
            weaponframes[Mace_Bash_Left_Execute, 4] = -1; weaponframes[Mace_Bash_Left_Execute, 5] = -1;

            weaponframes[Mace_Stab_Left_Charge, 0] = 213; weaponframes[Mace_Stab_Left_Charge, 1] = 214;
            weaponframes[Mace_Stab_Left_Charge, 2] = 215; weaponframes[Mace_Stab_Left_Charge, 3] = -1;
            weaponframes[Mace_Stab_Left_Charge, 4] = -1; weaponframes[Mace_Stab_Left_Charge, 5] = -1;

            weaponframes[Mace_Stab_Left_Execute, 0] = 208; weaponframes[Mace_Stab_Left_Execute, 1] = 209;
            weaponframes[Mace_Stab_Left_Execute, 2] = 210; weaponframes[Mace_Stab_Left_Execute, 3] = 210;
            weaponframes[Mace_Stab_Left_Execute, 4] = 209; weaponframes[Mace_Stab_Left_Execute, 5] = 208;

            weaponframes[Mace_Ready_Left, 0] = 213; weaponframes[Mace_Ready_Left, 1] = -1;
            weaponframes[Mace_Ready_Left, 2] = -1; weaponframes[Mace_Ready_Left, 3] = -1;
            weaponframes[Mace_Ready_Left, 4] = -1; weaponframes[Mace_Ready_Left, 5] = -1;

            weaponframes[Fist_Slash_Left_Charge, 0] = 227; weaponframes[Fist_Slash_Left_Charge, 1] = -1;
            weaponframes[Fist_Slash_Left_Charge, 2] = -1; weaponframes[Fist_Slash_Left_Charge, 3] = -1;
            weaponframes[Fist_Slash_Left_Charge, 4] = -1; weaponframes[Fist_Slash_Left_Charge, 5] = -1;

            weaponframes[Fist_Slash_Left_Execute, 0] = 221; weaponframes[Fist_Slash_Left_Execute, 1] = 222;
            weaponframes[Fist_Slash_Left_Execute, 2] = 223; weaponframes[Fist_Slash_Left_Execute, 3] = -1;
            weaponframes[Fist_Slash_Left_Execute, 4] = -1; weaponframes[Fist_Slash_Left_Execute, 5] = -1;

            weaponframes[Fist_Bash_Left_Charge, 0] = 227; weaponframes[Fist_Bash_Left_Charge, 1] = -1;
            weaponframes[Fist_Bash_Left_Charge, 2] = -1; weaponframes[Fist_Bash_Left_Charge, 3] = -1;
            weaponframes[Fist_Bash_Left_Charge, 4] = -1; weaponframes[Fist_Bash_Left_Charge, 5] = -1;

            weaponframes[Fist_Bash_Left_Execute, 0] = 221; weaponframes[Fist_Bash_Left_Execute, 1] = 222;
            weaponframes[Fist_Bash_Left_Execute, 2] = 223; weaponframes[Fist_Bash_Left_Execute, 3] = -1;
            weaponframes[Fist_Bash_Left_Execute, 4] = -1; weaponframes[Fist_Bash_Left_Execute, 5] = -1;

            weaponframes[Fist_Stab_Left_Charge, 0] = 227; weaponframes[Fist_Stab_Left_Charge, 1] = -1;
            weaponframes[Fist_Stab_Left_Charge, 2] = -1; weaponframes[Fist_Stab_Left_Charge, 3] = -1;
            weaponframes[Fist_Stab_Left_Charge, 4] = -1; weaponframes[Fist_Stab_Left_Charge, 5] = -1;

            weaponframes[Fist_Stab_Left_Execute, 0] = 221; weaponframes[Fist_Stab_Left_Execute, 1] = 222;
            weaponframes[Fist_Stab_Left_Execute, 2] = 223; weaponframes[Fist_Stab_Left_Execute, 3] = -1;
            weaponframes[Fist_Stab_Left_Execute, 4] = -1; weaponframes[Fist_Stab_Left_Execute, 5] = -1;

            weaponframes[Fist_Ready_Left, 0] = 225; weaponframes[Fist_Ready_Left, 1] = -1;
            weaponframes[Fist_Ready_Left, 2] = -1; weaponframes[Fist_Ready_Left, 3] = -1;
            weaponframes[Fist_Ready_Left, 4] = -1; weaponframes[Fist_Ready_Left, 5] = -1;
        }

    }   //end class
}//end namespace