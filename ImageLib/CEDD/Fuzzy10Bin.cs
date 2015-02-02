
// 
/*
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 * 
 * Savvas Chatzichristofis
 * Download the latest version from http://www.chatzichristofis.info
 * 
 * Details regarding these descriptors can be found at the following papers: (in other words, if you use these descriptors in your scientific work, we kindly ask you to cite one or more of the following papers  )
 *
 * S. A. Chatzichristofis and Y. S. Boutalis, “CEDD: COLOR AND EDGE DIRECTIVITY DESCRIPTOR – A COMPACT DESCRIPTOR FOR IMAGE INDEXING AND RETRIEVAL.”, «6th International Conference in advanced research on Computer Vision Systems (ICVS)», Lecture Notes in Computer Science (LNCS), pp.312-322, May 12 to 15, 2008, Santorini, Greece.
 *
 * S. A. Chatzichristofis, Y. S. Boutalis and M. Lux, “SELECTION OF THE PROPER COMPACT COMPOSIDE DESCRIPTOR FOR IMPROVING CONTENT BASED IMAGE RETRIEVAL.”, «The Sixth IASTED International Conference on Signal Processing, Pattern Recognition and Applications (SPPRA)», ACTA PRESS, pp.134-140, February 17 to 19, 2009, Innsbruck, Austria.
 * 
 * 
 * 
 * VER 1.01 April 1st 2013
 * schatzic@ee.duth.gr
 *

 */

using System;
using System.Collections.Generic;
using System.Text;

namespace CEDD_Descriptor

{
    class Fuzzy10Bin
    {
        public bool KeepPreviuesValues = false;



        protected double[] HueMembershipValues = new double[32] { 0,0,5, 10,
               5,10,35,50,
               35,50,70, 85, 
               70,85,150, 165, 
               150,165,195, 205,
               195,205,265, 280,
               265,280,315, 330, 
               315,330,360,360 }; // Table Dimensions= Number Of Triangles X 4 (Start - Stop)

        protected double[] SaturationMembershipValues = new double[8]      { 0,0,10, 75,
              10,75,255,255 };

        protected double[] ValueMembershipValues = new double[12] { 0,0,10, 75, 
               10,75,180, 220,
               180,220,255,255 };

        public struct FuzzyRules
        {
            public int Input1;
            public int Input2;
            public int Input3;
            public int Output;

        }

        public FuzzyRules[] Fuzzy10BinRules = new FuzzyRules[48];
        
        public double[] Fuzzy10BinHisto = new double[10];
        public double[] HueActivation = new double[8];
        public double[] SaturationActivation = new double[2];
        public double[] ValueActivation = new double[3];

        public int[,] Fuzzy10BinRulesDefinition = new int[48, 4]{
                          {0,0,0,2},                          
                          {0,1,0,2},
                          {0,0,2,0},
                          {0,0,1,1},
                          {1,0,0,2},                          
                          {1,1,0,2},
                          {1,0,2,0},
                          {1,0,1,1},
                          {2,0,0,2},                          
                          {2,1,0,2},
                          {2,0,2,0},
                          {2,0,1,1},
                          {3,0,0,2},                          
                          {3,1,0,2},
                          {3,0,2,0},
                          {3,0,1,1},
                          {4,0,0,2},                          
                          {4,1,0,2},
                          {4,0,2,0},
                          {4,0,1,1},
                          {5,0,0,2},                          
                          {5,1,0,2},
                          {5,0,2,0},
                          {5,0,1,1},
                          {6,0,0,2},                          
                          {6,1,0,2},
                          {6,0,2,0},
                          {6,0,1,1},
                          {7,0,0,2},                          
                          {7,1,0,2},
                          {7,0,2,0},
                          {7,0,1,1},
                          {0,1,1,3},
                          {0,1,2,3},                     
                          {1,1,1,4},
                          {1,1,2,4},
                          {2,1,1,5},
                          {2,1,2,5},
                          {3,1,1,6},
                          {3,1,2,6},
                          {4,1,1,7},
                          {4,1,2,7},
                          {5,1,1,8},
                          {5,1,2,8},
                          {6,1,1,9},
                          {6,1,2,9},
                          {7,1,1,3},
                          {7,1,2,3}
                          };  // 48 0 Αριθμός των κανόνων και 4 οι 3 είσοδοι και η μία έξοδος
                              // 

        public Fuzzy10Bin(bool KeepPreviuesValues )
        {
            for (int R = 0; R < 48; R++)
            {

                Fuzzy10BinRules[R].Input1 = Fuzzy10BinRulesDefinition[R, 0];
                Fuzzy10BinRules[R].Input2 = Fuzzy10BinRulesDefinition[R, 1];
                Fuzzy10BinRules[R].Input3 = Fuzzy10BinRulesDefinition[R, 2];
                Fuzzy10BinRules[R].Output = Fuzzy10BinRulesDefinition[R, 3];

            }

            this.KeepPreviuesValues = KeepPreviuesValues;

              
        }




        private void FindMembershipValueForTriangles(double Input, double[] Triangles, double[] MembershipFunctionToSave)
        {
            int Temp = 0;

            for (int i = 0; i <= Triangles.Length - 1; i += 4)
            {

                MembershipFunctionToSave[Temp] = 0;

                //Αν είναι ακριβός στη κορυφή
                if (Input >= Triangles[i + 1] && Input <= +Triangles[i + 2])
                {
                    MembershipFunctionToSave[Temp] = 1;
                }

                //Αν είναι δεξιά του τριγώνου    
                if (Input >= Triangles[i] && Input < Triangles[i + 1])
                {
                    MembershipFunctionToSave[Temp] = (Input - Triangles[i]) / (Triangles[i + 1] - Triangles[i]);
                }

                //Αν είναι αριστερα του τριγώνου    

                if (Input > Triangles[i + 2] && Input <= Triangles[i + 3])
                {
                    MembershipFunctionToSave[Temp] = (Input - Triangles[i + 2]) / (Triangles[i + 2] - Triangles[i + 3]) + 1;
                }

                Temp += 1;
            }

        }


        private void LOM_Defazzificator(FuzzyRules[] Rules, double[] Input1, double[] Input2, double[] Input3, double[] ResultTable)
        {
            int RuleActivation = -1;
            double LOM_MAXofMIN = 0;

            for (int i = 0; i < Rules.Length; i++)
            {

                if ((Input1[Rules[i].Input1] > 0) && (Input2[Rules[i].Input2] > 0) && (Input3[Rules[i].Input3] > 0))
                {

                    double Min = 0;
                    Min = Math.Min(Input1[Rules[i].Input1], Math.Min(Input2[Rules[i].Input2], Input3[Rules[i].Input3]));

                    if (Min > LOM_MAXofMIN)
                    {
                        LOM_MAXofMIN = Min;
                        RuleActivation = Rules[i].Output;
                    }

                }

            }


                ResultTable[RuleActivation]++;

         
        }


        private void MultiParticipate_Equal_Defazzificator(FuzzyRules[] Rules, double[] Input1, double[] Input2, double[] Input3, double[] ResultTable)
        {

            int RuleActivation = -1;

            for (int i = 0; i < Rules.Length; i++)
            {
                    if ((Input1[Rules[i].Input1] > 0) && (Input2[Rules[i].Input2] > 0) && (Input3[Rules[i].Input3] > 0))
                    {
                        RuleActivation = Rules[i].Output;
                        ResultTable[RuleActivation]++;

                    }

            }
        }


        private void MultiParticipate_Defazzificator(FuzzyRules[] Rules, double[] Input1, double[] Input2, double[] Input3, double[] ResultTable)
        {

            int RuleActivation = -1;

            for (int i = 0; i < Rules.Length; i++)
            {
                if ((Input1[Rules[i].Input1] > 0) && (Input2[Rules[i].Input2] > 0) && (Input3[Rules[i].Input3] > 0))
                {
                    RuleActivation = Rules[i].Output;
                    double Min = 0;
                    Min = Math.Min(Input1[Rules[i].Input1], Math.Min(Input2[Rules[i].Input2], Input3[Rules[i].Input3]));

                    ResultTable[RuleActivation] += Min;

                }

            }
        }

        public double[] ApplyFilter(double Hue, double Saturation, double Value, int Method)
        {
            // Method   0 = LOM
            //          1 = Multi Equal Participate
            //          2 = Multi Participate

            if (KeepPreviuesValues == false)
            {
                for (int i = 0; i < 10; i++)
                {
                    Fuzzy10BinHisto[i] = 0;
                }

            }

            FindMembershipValueForTriangles(Hue, HueMembershipValues, HueActivation);
            FindMembershipValueForTriangles(Saturation, SaturationMembershipValues, SaturationActivation);
            FindMembershipValueForTriangles(Value, ValueMembershipValues, ValueActivation);


            if (Method == 0) LOM_Defazzificator(Fuzzy10BinRules, HueActivation, SaturationActivation, ValueActivation, Fuzzy10BinHisto);
            if (Method == 1) MultiParticipate_Equal_Defazzificator(Fuzzy10BinRules, HueActivation, SaturationActivation, ValueActivation, Fuzzy10BinHisto);
            if (Method == 2) MultiParticipate_Defazzificator(Fuzzy10BinRules, HueActivation, SaturationActivation, ValueActivation, Fuzzy10BinHisto);

            return (Fuzzy10BinHisto);

        }
    }
}
