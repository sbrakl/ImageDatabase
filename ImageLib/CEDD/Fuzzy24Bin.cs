
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
    class Fuzzy24Bin
    {
        public double[] ResultsTable = new double[3];
        public double[] Fuzzy24BinHisto = new double[24];
        public bool KeepPreviusValues = false;

        protected double[] SaturationMembershipValues = new double[8] {  0,0,68, 188,  
               68,188,255, 255};

        protected double[] ValueMembershipValues = new double[8] {  0,0,68, 188, 
             68,188,255, 255};

        //  protected static double[] ValueMembershipValues = new double[8] {  0,0,68, 188,  
        //        50,138,255, 255};


        public struct FuzzyRules
        {
            public int Input1;
            public int Input2;
            public int Output;

        }

        public FuzzyRules[] Fuzzy24BinRules = new FuzzyRules[4];

        public double[] SaturationActivation = new double[2];
        public double[] ValueActivation = new double[2];

        public int[,] Fuzzy24BinRulesDefinition = new int[4, 3]{
                          {1,1,1},
                          {0,0,2},                     
                          {0,1,0},
                          {1,0,2}
                          };


        public Fuzzy24Bin(bool KeepPreviuesValues)
        {
            for (int R = 0; R < 4; R++)
            {

                Fuzzy24BinRules[R].Input1 = Fuzzy24BinRulesDefinition[R, 0];
                Fuzzy24BinRules[R].Input2 = Fuzzy24BinRulesDefinition[R, 1];
                Fuzzy24BinRules[R].Output = Fuzzy24BinRulesDefinition[R, 2];

            }

            this.KeepPreviusValues = KeepPreviuesValues;

              
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

        private void LOM_Defazzificator(FuzzyRules[] Rules, double[] Input1, double[] Input2,  double[] ResultTable)
        {
            int RuleActivation = -1;
            double LOM_MAXofMIN = 0;

            for (int i = 0; i < Rules.Length; i++)
            {

                if ((Input1[Rules[i].Input1] > 0) && (Input2[Rules[i].Input2] > 0) )
                {

                    double Min = 0;
                    Min = Math.Min(Input1[Rules[i].Input1],Input2[Rules[i].Input2]);

                    if (Min > LOM_MAXofMIN)
                    {
                        LOM_MAXofMIN = Min;
                        RuleActivation = Rules[i].Output;
                    }

                }

            }


            ResultTable[RuleActivation]++;


        }
        
        private void MultiParticipate_Equal_Defazzificator(FuzzyRules[] Rules, double[] Input1, double[] Input2, double[] ResultTable)
        {

            int RuleActivation = -1;

            for (int i = 0; i < Rules.Length; i++)
            {
                if ((Input1[Rules[i].Input1] > 0) && (Input2[Rules[i].Input2] > 0) )
                {
                    RuleActivation = Rules[i].Output;
                    ResultTable[RuleActivation]++;

                }

            }
        }

        private void MultiParticipate_Defazzificator(FuzzyRules[] Rules, double[] Input1, double[] Input2, double[] ResultTable)
        {

            int RuleActivation = -1;
            double Min = 0;
            for (int i = 0; i < Rules.Length; i++)
            {
                if ((Input1[Rules[i].Input1] > 0) && (Input2[Rules[i].Input2] > 0) )
                {
                    Min = Math.Min(Input1[Rules[i].Input1], Input2[Rules[i].Input2]);

                    RuleActivation = Rules[i].Output;
                    ResultTable[RuleActivation] += Min;

                }

            }
        }


        public double[] ApplyFilter(double Hue, double Saturation, double Value,double[] ColorValues, int Method)
        {
            // Method   0 = LOM
            //          1 = Multi Equal Participate
            //          2 = Multi Participate

            ResultsTable[0] = 0;
            ResultsTable[1] = 0;
            ResultsTable[2] = 0;
            double Temp = 0;


            FindMembershipValueForTriangles(Saturation, SaturationMembershipValues, SaturationActivation);
            FindMembershipValueForTriangles(Value, ValueMembershipValues, ValueActivation);


            if (this.KeepPreviusValues   == false)
            {
                for (int i = 0; i < 24; i++)
                {
                    Fuzzy24BinHisto[i] = 0;
                }

            }

            for (int i = 3; i < 10; i++)
            {
                Temp += ColorValues[i];
            }

            if (Temp > 0)
            {
                if (Method == 0) LOM_Defazzificator(Fuzzy24BinRules, SaturationActivation, ValueActivation, ResultsTable);
                if (Method == 1) MultiParticipate_Equal_Defazzificator(Fuzzy24BinRules, SaturationActivation, ValueActivation, ResultsTable);
                if (Method == 2) MultiParticipate_Defazzificator(Fuzzy24BinRules, SaturationActivation, ValueActivation, ResultsTable);


            }

            for (int i = 0; i < 3; i++)
            {
                Fuzzy24BinHisto[i] += ColorValues[i];
            }


            for (int i = 3; i < 10; i++)
            {
                Fuzzy24BinHisto[(i - 2) * 3] += ColorValues[i] * ResultsTable[0];
                Fuzzy24BinHisto[(i - 2) * 3+1] += ColorValues[i] * ResultsTable[1];
                Fuzzy24BinHisto[(i - 2) * 3+2] += ColorValues[i] * ResultsTable[2];
            }

            return (Fuzzy24BinHisto);

        }


    }
}
