using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.Formula
{
    [ComVisible(false)]
    internal static class ElementsLoader
    {
        // Ignore Spelling: csv, txt, frm, isoData
        // Ignore Spelling: Sg, Bh, Hs, Ds, Rg, Cn, Nh, Fl, Mc, Lv, Og

        /// <summary>
        /// Loads the element data and return it.
        /// </summary>
        public static ElementMem[] MemoryLoadElements()
        {
            var elementData = new ElementMem[ElementsAndAbbrevs.ELEMENT_COUNT + 1];

            // Data obtained from https://www.nist.gov/pml/atomic-weights-and-isotopic-compositions-relative-atomic-masses
            // which obtained its data from https://www.ciaaw.org/atomic-weights.htm and https://www.degruyter.com/document/doi/10.1351/PAC-REP-10-06-02/html

            // For elements with a standard atomic weight range (e.g. [6.938,6.997] for Lithium), use the conventional atomic-weight,
            // as defined in Table 3 in "Atomic weights of the elements 2013 (IUPAC Technical Report)"
            // Published in Pure and Applied Chemistry, Volume 88, Issue 3
            // https://doi.org/10.1515/pac-2015-0305

            // Average mass values and/or uncertainties for 14 elements were further revised in 2018
            // as described in "Standard Atomic Weights of 14 Chemical Elements Revised"
            // Published in Chemistry International, Volume 40, Issue 4
            // https://doi.org/10.1515/ci-2018-0409

            // For radioactive elements, the mass of the most stable isotope is stored for the isotopic mass

            // Naturally occurring radioactive elements have an average weight and associated uncertainty
            // For the other radioactive elements, the mass of the most stable isotope is used, rounded to one decimal place
            // When an average mass uncertainty is not available, a value of 0.0005 is used

            // For example, Nobelium has average mass 259.1 (±0.0005)

            // Assigning element name, Charge (oxidation state), integer mass (of the isotopic mass), isotopic weight, average mass, and average mass uncertainty
            elementData[1] = new ElementMem("H", 1, 1, 1.00782503223, 1.008, 0.000135);
            elementData[2] = new ElementMem("He", 0, 4, 4.00260325413, 4.002602, 0.000002);
            elementData[3] = new ElementMem("Li", 1, 7, 7.0160034366, 6.94, 0.0295);
            elementData[4] = new ElementMem("Be", 2, 9, 9.012183065, 9.0121831, 0.0000005);
            elementData[5] = new ElementMem("B", 3, 11, 11.00930536, 10.81, 0.0075);
            elementData[6] = new ElementMem("C", 4, 12, 12, 12.011, 0.001);
            elementData[7] = new ElementMem("N", -3, 14, 14.00307400443, 14.007, 0.000425);
            elementData[8] = new ElementMem("O", -2, 16, 15.99491461957, 15.999, 0.00037);
            elementData[9] = new ElementMem("F", -1, 19, 18.99840316273, 18.998403163, 0.000000006);
            elementData[10] = new ElementMem("Ne", 0, 20, 19.9924401762, 20.1797, 0.0006);
            elementData[11] = new ElementMem("Na", 1, 23, 22.989769282, 22.98976928, 0.00000002);
            elementData[12] = new ElementMem("Mg", 2, 24, 23.985041697, 24.305, 0.0015);
            elementData[13] = new ElementMem("Al", 3, 27, 26.98153853, 26.9815384, 0.0000003);
            elementData[14] = new ElementMem("Si", 4, 28, 27.97692653465, 28.085, 0.001);
            elementData[15] = new ElementMem("P", -3, 31, 30.97376199842, 30.973761998, 0.000000005);
            elementData[16] = new ElementMem("S", -2, 32, 31.9720711744, 32.06, 0.0085);
            elementData[17] = new ElementMem("Cl", -1, 35, 34.968852682, 35.45, 0.0055);
            elementData[18] = new ElementMem("Ar", 0, 40, 39.9623831237, 39.95, 0.0855);
            elementData[19] = new ElementMem("K", 1, 39, 38.9637064864, 39.0983, 0.0001);
            elementData[20] = new ElementMem("Ca", 2, 40, 39.962590863, 40.078, 0.004);
            elementData[21] = new ElementMem("Sc", 3, 45, 44.95590828, 44.955908, 0.000005);
            elementData[22] = new ElementMem("Ti", 4, 48, 47.94794198, 47.867, 0.001);
            elementData[23] = new ElementMem("V", 5, 51, 50.94395704, 50.9415, 0.0001);
            elementData[24] = new ElementMem("Cr", 3, 52, 51.94050623, 51.9961, 0.0006);
            elementData[25] = new ElementMem("Mn", 2, 55, 54.93804391, 54.938043, 0.000002);
            elementData[26] = new ElementMem("Fe", 3, 56, 55.93493633, 55.845, 0.002);
            elementData[27] = new ElementMem("Co", 2, 59, 58.93319429, 58.933194, 0.000003);
            elementData[28] = new ElementMem("Ni", 2, 58, 57.93534241, 58.6934, 0.0004);
            elementData[29] = new ElementMem("Cu", 2, 63, 62.92959772, 63.546, 0.003);
            elementData[30] = new ElementMem("Zn", 2, 64, 63.92914201, 65.38, 0.02);
            elementData[31] = new ElementMem("Ga", 3, 69, 68.9255735, 69.723, 0.001);
            elementData[32] = new ElementMem("Ge", 4, 72, 71.922075826, 72.63, 0.008);
            elementData[33] = new ElementMem("As", -3, 75, 74.92159457, 74.921595, 0.000006);
            elementData[34] = new ElementMem("Se", -2, 80, 79.9165218, 78.971, 0.008);
            elementData[35] = new ElementMem("Br", -1, 79, 78.9183376, 79.904, 0.003);
            elementData[36] = new ElementMem("Kr", 0, 84, 83.9114977282, 83.798, 0.002);
            elementData[37] = new ElementMem("Rb", 1, 85, 84.9117897379, 85.4678, 0.0003);
            elementData[38] = new ElementMem("Sr", 2, 88, 87.9056125, 87.62, 0.01);
            elementData[39] = new ElementMem("Y", 3, 89, 88.9058403, 88.90584, 0.00001);
            elementData[40] = new ElementMem("Zr", 4, 90, 89.9046977, 91.224, 0.002);
            elementData[41] = new ElementMem("Nb", 5, 93, 92.906373, 92.90637, 0.00001);
            elementData[42] = new ElementMem("Mo", 6, 98, 97.90540482, 95.95, 0.01);
            elementData[43] = new ElementMem("Tc", 7, 98, 97.9072124, 98, 0.0005);
            elementData[44] = new ElementMem("Ru", 4, 102, 101.9043441, 101.07, 0.02);
            elementData[45] = new ElementMem("Rh", 3, 103, 102.905498, 102.90549, 0.00002);
            elementData[46] = new ElementMem("Pd", 2, 106, 105.9034804, 106.42, 0.01);
            elementData[47] = new ElementMem("Ag", 1, 107, 106.9050916, 107.8682, 0.0002);
            elementData[48] = new ElementMem("Cd", 2, 114, 113.90336509, 112.414, 0.004);
            elementData[49] = new ElementMem("In", 3, 115, 114.903878776, 114.818, 0.001);
            elementData[50] = new ElementMem("Sn", 4, 120, 119.90220163, 118.71, 0.007);
            elementData[51] = new ElementMem("Sb", -3, 121, 120.903812, 121.76, 0.001);
            elementData[52] = new ElementMem("Te", -2, 130, 129.906222748, 127.6, 0.03);
            elementData[53] = new ElementMem("I", -1, 127, 126.9044719, 126.90447, 0.00003);
            elementData[54] = new ElementMem("Xe", 0, 132, 131.9041550856, 131.293, 0.006);
            elementData[55] = new ElementMem("Cs", 1, 133, 132.905451961, 132.90545196, 0.00000006);
            elementData[56] = new ElementMem("Ba", 2, 138, 137.905247, 137.327, 0.007);
            elementData[57] = new ElementMem("La", 3, 139, 138.9063563, 138.90547, 0.00007);
            elementData[58] = new ElementMem("Ce", 3, 140, 139.9054431, 140.116, 0.001);
            elementData[59] = new ElementMem("Pr", 4, 141, 140.9076576, 140.90766, 0.00001);
            elementData[60] = new ElementMem("Nd", 3, 142, 141.907729, 144.242, 0.003);
            elementData[61] = new ElementMem("Pm", 3, 145, 144.9127559, 145, 0.0005);
            elementData[62] = new ElementMem("Sm", 3, 152, 151.9197397, 150.36, 0.02);
            elementData[63] = new ElementMem("Eu", 3, 153, 152.921238, 151.964, 0.001);
            elementData[64] = new ElementMem("Gd", 3, 158, 157.9241123, 157.25, 0.03);
            elementData[65] = new ElementMem("Tb", 3, 159, 158.9253547, 158.925354, 0.000008);
            elementData[66] = new ElementMem("Dy", 3, 164, 163.9291819, 162.5, 0.001);
            elementData[67] = new ElementMem("Ho", 3, 165, 164.9303288, 164.930328, 0.000007);
            elementData[68] = new ElementMem("Er", 3, 166, 165.9302995, 167.259, 0.003);
            elementData[69] = new ElementMem("Tm", 3, 169, 168.9342179, 168.934218, 0.000006);
            elementData[70] = new ElementMem("Yb", 3, 174, 173.9388664, 173.054, 0.005);
            elementData[71] = new ElementMem("Lu", 3, 175, 174.9407752, 174.9668, 0.0001);
            elementData[72] = new ElementMem("Hf", 4, 180, 179.946557, 178.49, 0.02);
            elementData[73] = new ElementMem("Ta", 5, 181, 180.9479958, 180.94788, 0.00002);
            elementData[74] = new ElementMem("W", 6, 184, 183.95093092, 183.84, 0.01);
            elementData[75] = new ElementMem("Re", 7, 187, 186.9557501, 186.207, 0.001);
            elementData[76] = new ElementMem("Os", 4, 192, 191.961477, 190.23, 0.03);
            elementData[77] = new ElementMem("Ir", 4, 193, 192.9629216, 192.217, 0.002);
            elementData[78] = new ElementMem("Pt", 4, 195, 194.9647917, 195.084, 0.009);
            elementData[79] = new ElementMem("Au", 3, 197, 196.96656879, 196.96657, 0.00004);
            elementData[80] = new ElementMem("Hg", 2, 202, 201.9706434, 200.592, 0.003);
            elementData[81] = new ElementMem("Tl", 1, 205, 204.9744278, 204.38, 0.0015);
            elementData[82] = new ElementMem("Pb", 2, 208, 207.9766525, 207.2, 0.1);
            elementData[83] = new ElementMem("Bi", 3, 209, 208.9803991, 208.9804, 0.00001);
            elementData[84] = new ElementMem("Po", 4, 209, 208.9824308, 209, 0.0005);
            elementData[85] = new ElementMem("At", -1, 210, 209.9871479, 210, 0.0005);
            elementData[86] = new ElementMem("Rn", 0, 222, 222.0175782, 222, 0.0005);
            elementData[87] = new ElementMem("Fr", 1, 223, 223.019736, 223, 0.0005);
            elementData[88] = new ElementMem("Ra", 2, 226, 226.0254103, 226, 0.0005);
            elementData[89] = new ElementMem("Ac", 3, 227, 227.0277523, 227, 0.0005);
            elementData[90] = new ElementMem("Th", 4, 232, 232.0380558, 232.0377, 0.0004);
            elementData[91] = new ElementMem("Pa", 5, 231, 231.0358842, 231.03588, 0.00001);
            elementData[92] = new ElementMem("U", 6, 238, 238.0507884, 238.02891, 0.00003);
            elementData[93] = new ElementMem("Np", 5, 237, 237.0481736, 237, 0.0005);
            elementData[94] = new ElementMem("Pu", 4, 244, 244.0642053, 244, 0.0005);
            elementData[95] = new ElementMem("Am", 3, 243, 243.0613813, 243.1, 0.0005);
            elementData[96] = new ElementMem("Cm", 3, 247, 247.0703541, 247.1, 0.0005);
            elementData[97] = new ElementMem("Bk", 3, 247, 247.0703073, 247.1, 0.0005);
            elementData[98] = new ElementMem("Cf", 3, 251, 251.0795886, 251.1, 0.0005);
            elementData[99] = new ElementMem("Es", 3, 252, 252.08298, 252.1, 0.0005);
            elementData[100] = new ElementMem("Fm", 3, 257, 257.0951061, 257.1, 0.0005);
            elementData[101] = new ElementMem("Md", 3, 258, 258.0984315, 258.1, 0.0005);
            elementData[102] = new ElementMem("No", 2, 259, 259.10103, 259.1, 0.0005);
            elementData[103] = new ElementMem("Lr", 3, 262, 262.10961, 262.1, 0.0005);
            elementData[104] = new ElementMem("Rf", 4, 267, 267.12179, 267.1, 0.0005);
            elementData[105] = new ElementMem("Db", 5, 268, 268.12567, 268.1, 0.0005);
            elementData[106] = new ElementMem("Sg", 6, 271, 271.13393, 271.1, 0.0005);
            elementData[107] = new ElementMem("Bh", 7, 272, 272.13826, 272.1, 0.0005);
            elementData[108] = new ElementMem("Hs", 8, 270, 270.13429, 270.1, 0.0005);
            elementData[109] = new ElementMem("Mt", 3, 276, 276.15159, 276.2, 0.0005);
            elementData[110] = new ElementMem("Ds", 0, 281, 281.16451, 281.2, 0.0005);
            elementData[111] = new ElementMem("Rg", 3, 280, 280.16514, 280.2, 0.0005);
            elementData[112] = new ElementMem("Cn", 2, 285, 285.17712, 285.2, 0.0005);
            elementData[113] = new ElementMem("Nh", 1, 284, 284.17873, 284.2, 0.0005);
            elementData[114] = new ElementMem("Fl", 2, 289, 289.19042, 289.2, 0.0005);
            elementData[115] = new ElementMem("Mc", 1, 288, 288.19274, 288.2, 0.0005);
            elementData[116] = new ElementMem("Lv", 2, 293, 293.20449, 293.2, 0.0005);
            elementData[117] = new ElementMem("Ts", 1, 292, 292.20746, 292.2, 0.0005);
            elementData[118] = new ElementMem("Og", 2, 294, 294.21392, 294.2, 0.0005);

            return elementData;
        }

        /// <summary>
        /// Stores isotope information in elementStats[]
        /// </summary>
        internal static void MemoryLoadIsotopes(ElementInfo[] elementStats)
        {
            // The isoData[] array holds the list of isotopes for each element
            // Create the lists now, to simplify the code below.
            var isoData = new List<IsotopeInfo>[ElementsAndAbbrevs.ELEMENT_COUNT + 1];
            for (var i = 0; i <= ElementsAndAbbrevs.ELEMENT_COUNT; i++)
            {
                isoData[i] = new List<IsotopeInfo>(1);
            }

            // Data obtained from https://www.nist.gov/pml/atomic-weights-and-isotopic-compositions-relative-atomic-masses
            // which obtained its data from https://www.ciaaw.org/atomic-weights.htm and https://www.degruyter.com/document/doi/10.1351/PAC-REP-10-06-02/html

            // For radioactive elements that do not occur in nature, use the mass of the isotope with the longest half life

            isoData[1].Add(new IsotopeInfo(1.00782503223, 0.999885f));
            isoData[1].Add(new IsotopeInfo(2.01410177812, 0.000115f));
            isoData[2].Add(new IsotopeInfo(3.0160293201, 0.00000134f));
            isoData[2].Add(new IsotopeInfo(4.00260325413, 0.99999866f));
            isoData[3].Add(new IsotopeInfo(6.0151228874, 0.0759f));
            isoData[3].Add(new IsotopeInfo(7.0160034366, 0.9241f));
            isoData[4].Add(new IsotopeInfo(9.012183065, 1f));
            isoData[5].Add(new IsotopeInfo(10.01293695, 0.199f));
            isoData[5].Add(new IsotopeInfo(11.00930536, 0.801f));
            isoData[6].Add(new IsotopeInfo(12, 0.9893f));
            isoData[6].Add(new IsotopeInfo(13.00335483507, 0.0107f));
            isoData[7].Add(new IsotopeInfo(14.00307400443, 0.99636f));
            isoData[7].Add(new IsotopeInfo(15.00010889888, 0.00364f));
            isoData[8].Add(new IsotopeInfo(15.99491461957, 0.99757f));
            isoData[8].Add(new IsotopeInfo(16.9991317565, 0.00038f));
            isoData[8].Add(new IsotopeInfo(17.99915961286, 0.00205f));
            isoData[9].Add(new IsotopeInfo(18.99840316273, 1f));
            isoData[10].Add(new IsotopeInfo(19.9924401762, 0.9048f));
            isoData[10].Add(new IsotopeInfo(20.993846685, 0.0027f));
            isoData[10].Add(new IsotopeInfo(21.991385114, 0.0925f));
            isoData[11].Add(new IsotopeInfo(22.989769282, 1f));
            isoData[12].Add(new IsotopeInfo(23.985041697, 0.7899f));
            isoData[12].Add(new IsotopeInfo(24.985836976, 0.1f));
            isoData[12].Add(new IsotopeInfo(25.982592968, 0.1101f));
            isoData[13].Add(new IsotopeInfo(26.98153853, 1f));
            isoData[14].Add(new IsotopeInfo(27.97692653465, 0.92223f));
            isoData[14].Add(new IsotopeInfo(28.9764946649, 0.04685f));
            isoData[14].Add(new IsotopeInfo(29.973770136, 0.03092f));
            isoData[15].Add(new IsotopeInfo(30.97376199842, 1f));
            isoData[16].Add(new IsotopeInfo(31.9720711744, 0.9499f));
            isoData[16].Add(new IsotopeInfo(32.9714589098, 0.0075f));
            isoData[16].Add(new IsotopeInfo(33.967867004, 0.0425f));
            isoData[16].Add(new IsotopeInfo(35.96708071, 0.0001f));
            isoData[17].Add(new IsotopeInfo(34.968852682, 0.7576f));
            isoData[17].Add(new IsotopeInfo(36.965902602, 0.2424f));
            isoData[18].Add(new IsotopeInfo(35.967545105, 0.003336f));
            isoData[18].Add(new IsotopeInfo(37.96273211, 0.000629f));
            isoData[18].Add(new IsotopeInfo(39.9623831237, 0.996035f));
            isoData[19].Add(new IsotopeInfo(38.9637064864, 0.932581f));
            isoData[19].Add(new IsotopeInfo(39.963998166, 0.000117f));
            isoData[19].Add(new IsotopeInfo(40.9618252579, 0.067302f));
            isoData[20].Add(new IsotopeInfo(39.962590863, 0.96941f));
            isoData[20].Add(new IsotopeInfo(41.95861783, 0.00647f));
            isoData[20].Add(new IsotopeInfo(42.95876644, 0.00135f));
            isoData[20].Add(new IsotopeInfo(43.95548156, 0.02086f));
            isoData[20].Add(new IsotopeInfo(45.953689, 0.00004f));
            isoData[20].Add(new IsotopeInfo(47.95252276, 0.00187f));
            isoData[21].Add(new IsotopeInfo(44.95590828, 1f));
            isoData[22].Add(new IsotopeInfo(45.95262772, 0.0825f));
            isoData[22].Add(new IsotopeInfo(46.95175879, 0.0744f));
            isoData[22].Add(new IsotopeInfo(47.94794198, 0.7372f));
            isoData[22].Add(new IsotopeInfo(48.94786568, 0.0541f));
            isoData[22].Add(new IsotopeInfo(49.94478689, 0.0518f));
            isoData[23].Add(new IsotopeInfo(49.94715601, 0.0025f));
            isoData[23].Add(new IsotopeInfo(50.94395704, 0.9975f));
            isoData[24].Add(new IsotopeInfo(49.94604183, 0.04345f));
            isoData[24].Add(new IsotopeInfo(51.94050623, 0.83789f));
            isoData[24].Add(new IsotopeInfo(52.94064815, 0.09501f));
            isoData[24].Add(new IsotopeInfo(53.93887916, 0.02365f));
            isoData[25].Add(new IsotopeInfo(54.93804391, 1f));
            isoData[26].Add(new IsotopeInfo(53.93960899, 0.05845f));
            isoData[26].Add(new IsotopeInfo(55.93493633, 0.91754f));
            isoData[26].Add(new IsotopeInfo(56.93539284, 0.02119f));
            isoData[26].Add(new IsotopeInfo(57.93327443, 0.00282f));
            isoData[27].Add(new IsotopeInfo(58.93319429, 1f));
            isoData[28].Add(new IsotopeInfo(57.93534241, 0.68077f));
            isoData[28].Add(new IsotopeInfo(59.93078588, 0.26223f));
            isoData[28].Add(new IsotopeInfo(60.93105557, 0.011399f));
            isoData[28].Add(new IsotopeInfo(61.92834537, 0.036346f));
            isoData[28].Add(new IsotopeInfo(63.92796682, 0.009255f));
            isoData[29].Add(new IsotopeInfo(62.92959772, 0.6915f));
            isoData[29].Add(new IsotopeInfo(64.9277897, 0.3085f));
            isoData[30].Add(new IsotopeInfo(63.92914201, 0.4917f));
            isoData[30].Add(new IsotopeInfo(65.92603381, 0.2773f));
            isoData[30].Add(new IsotopeInfo(66.92712775, 0.0404f));
            isoData[30].Add(new IsotopeInfo(67.92484455, 0.1845f));
            isoData[30].Add(new IsotopeInfo(69.9253192, 0.0061f));
            isoData[31].Add(new IsotopeInfo(68.9255735, 0.60108f));
            isoData[31].Add(new IsotopeInfo(70.92470258, 0.39892f));
            isoData[32].Add(new IsotopeInfo(69.92424875, 0.2057f));
            isoData[32].Add(new IsotopeInfo(71.922075826, 0.2745f));
            isoData[32].Add(new IsotopeInfo(72.923458956, 0.0775f));
            isoData[32].Add(new IsotopeInfo(73.921177761, 0.365f));
            isoData[32].Add(new IsotopeInfo(75.921402726, 0.0773f));
            isoData[33].Add(new IsotopeInfo(74.92159457, 1f));
            isoData[34].Add(new IsotopeInfo(73.922475934, 0.0089f));
            isoData[34].Add(new IsotopeInfo(75.919213704, 0.0937f));
            isoData[34].Add(new IsotopeInfo(76.919914154, 0.0763f));
            isoData[34].Add(new IsotopeInfo(77.91730928, 0.2377f));
            isoData[34].Add(new IsotopeInfo(79.9165218, 0.4961f));
            isoData[34].Add(new IsotopeInfo(81.9166995, 0.0873f));
            isoData[35].Add(new IsotopeInfo(78.9183376, 0.5069f));
            isoData[35].Add(new IsotopeInfo(80.9162897, 0.4931f));
            isoData[36].Add(new IsotopeInfo(77.92036494, 0.00355f));
            isoData[36].Add(new IsotopeInfo(79.91637808, 0.02286f));
            isoData[36].Add(new IsotopeInfo(81.91348273, 0.11593f));
            isoData[36].Add(new IsotopeInfo(82.91412716, 0.115f));
            isoData[36].Add(new IsotopeInfo(83.9114977282, 0.56987f));
            isoData[36].Add(new IsotopeInfo(85.9106106269, 0.17279f));
            isoData[37].Add(new IsotopeInfo(84.9117897379, 0.7217f));
            isoData[37].Add(new IsotopeInfo(86.909180531, 0.2783f));
            isoData[38].Add(new IsotopeInfo(83.9134191, 0.0056f));
            isoData[38].Add(new IsotopeInfo(85.9092606, 0.0986f));
            isoData[38].Add(new IsotopeInfo(86.9088775, 0.07f));
            isoData[38].Add(new IsotopeInfo(87.9056125, 0.8258f));
            isoData[39].Add(new IsotopeInfo(88.9058403, 1f));
            isoData[40].Add(new IsotopeInfo(89.9046977, 0.5145f));
            isoData[40].Add(new IsotopeInfo(90.9056396, 0.1122f));
            isoData[40].Add(new IsotopeInfo(91.9050347, 0.1715f));
            isoData[40].Add(new IsotopeInfo(93.9063108, 0.1738f));
            isoData[40].Add(new IsotopeInfo(95.9082714, 0.028f));
            isoData[41].Add(new IsotopeInfo(92.906373, 1f));
            isoData[42].Add(new IsotopeInfo(91.90680796, 0.1453f));
            isoData[42].Add(new IsotopeInfo(93.9050849, 0.0915f));
            isoData[42].Add(new IsotopeInfo(94.90583877, 0.1584f));
            isoData[42].Add(new IsotopeInfo(95.90467612, 0.1667f));
            isoData[42].Add(new IsotopeInfo(96.90601812, 0.096f));
            isoData[42].Add(new IsotopeInfo(97.90540482, 0.2439f));
            isoData[42].Add(new IsotopeInfo(99.9074718, 0.0982f));
            isoData[43].Add(new IsotopeInfo(97.9072124, 1f));
            isoData[44].Add(new IsotopeInfo(95.90759025, 0.0554f));
            isoData[44].Add(new IsotopeInfo(97.9052868, 0.0187f));
            isoData[44].Add(new IsotopeInfo(98.9059341, 0.1276f));
            isoData[44].Add(new IsotopeInfo(99.9042143, 0.126f));
            isoData[44].Add(new IsotopeInfo(100.9055769, 0.1706f));
            isoData[44].Add(new IsotopeInfo(101.9043441, 0.3155f));
            isoData[44].Add(new IsotopeInfo(103.9054275, 0.1862f));
            isoData[45].Add(new IsotopeInfo(102.905498, 1f));
            isoData[46].Add(new IsotopeInfo(101.9056022, 0.0102f));
            isoData[46].Add(new IsotopeInfo(103.9040305, 0.1114f));
            isoData[46].Add(new IsotopeInfo(104.9050796, 0.2233f));
            isoData[46].Add(new IsotopeInfo(105.9034804, 0.2733f));
            isoData[46].Add(new IsotopeInfo(107.9038916, 0.2646f));
            isoData[46].Add(new IsotopeInfo(109.9051722, 0.1172f));
            isoData[47].Add(new IsotopeInfo(106.9050916, 0.51839f));
            isoData[47].Add(new IsotopeInfo(108.9047553, 0.48161f));
            isoData[48].Add(new IsotopeInfo(105.9064599, 0.0125f));
            isoData[48].Add(new IsotopeInfo(107.9041834, 0.0089f));
            isoData[48].Add(new IsotopeInfo(109.90300661, 0.1249f));
            isoData[48].Add(new IsotopeInfo(110.90418287, 0.128f));
            isoData[48].Add(new IsotopeInfo(111.90276287, 0.2413f));
            isoData[48].Add(new IsotopeInfo(112.90440813, 0.1222f));
            isoData[48].Add(new IsotopeInfo(113.90336509, 0.2873f));
            isoData[48].Add(new IsotopeInfo(115.90476315, 0.0749f));
            isoData[49].Add(new IsotopeInfo(112.90406184, 0.0429f));
            isoData[49].Add(new IsotopeInfo(114.903878776, 0.9571f));
            isoData[50].Add(new IsotopeInfo(111.90482387, 0.0097f));
            isoData[50].Add(new IsotopeInfo(113.9027827, 0.0066f));
            isoData[50].Add(new IsotopeInfo(114.903344699, 0.0034f));
            isoData[50].Add(new IsotopeInfo(115.9017428, 0.1454f));
            isoData[50].Add(new IsotopeInfo(116.90295398, 0.0768f));
            isoData[50].Add(new IsotopeInfo(117.90160657, 0.2422f));
            isoData[50].Add(new IsotopeInfo(118.90331117, 0.0859f));
            isoData[50].Add(new IsotopeInfo(119.90220163, 0.3258f));
            isoData[50].Add(new IsotopeInfo(121.9034438, 0.0463f));
            isoData[50].Add(new IsotopeInfo(123.9052766, 0.0579f));
            isoData[51].Add(new IsotopeInfo(120.903812, 0.5721f));
            isoData[51].Add(new IsotopeInfo(122.9042132, 0.4279f));
            isoData[52].Add(new IsotopeInfo(119.9040593, 0.0009f));
            isoData[52].Add(new IsotopeInfo(121.9030435, 0.0255f));
            isoData[52].Add(new IsotopeInfo(122.9042698, 0.0089f));
            isoData[52].Add(new IsotopeInfo(123.9028171, 0.0474f));
            isoData[52].Add(new IsotopeInfo(124.9044299, 0.0707f));
            isoData[52].Add(new IsotopeInfo(125.9033109, 0.1884f));
            isoData[52].Add(new IsotopeInfo(127.90446128, 0.3174f));
            isoData[52].Add(new IsotopeInfo(129.906222748, 0.3408f));
            isoData[53].Add(new IsotopeInfo(126.9044719, 1f));
            isoData[54].Add(new IsotopeInfo(123.905892, 0.000952f));
            isoData[54].Add(new IsotopeInfo(125.9042983, 0.00089f));
            isoData[54].Add(new IsotopeInfo(127.903531, 0.019102f));
            isoData[54].Add(new IsotopeInfo(128.9047808611, 0.264006f));
            isoData[54].Add(new IsotopeInfo(129.903509349, 0.04071f));
            isoData[54].Add(new IsotopeInfo(130.90508406, 0.212324f));
            isoData[54].Add(new IsotopeInfo(131.9041550856, 0.269086f));
            isoData[54].Add(new IsotopeInfo(133.90539466, 0.104357f));
            isoData[54].Add(new IsotopeInfo(135.907214484, 0.088573f));
            isoData[55].Add(new IsotopeInfo(132.905451961, 1f));
            isoData[56].Add(new IsotopeInfo(129.9063207, 0.00106f));
            isoData[56].Add(new IsotopeInfo(131.9050611, 0.00101f));
            isoData[56].Add(new IsotopeInfo(133.90450818, 0.02417f));
            isoData[56].Add(new IsotopeInfo(134.90568838, 0.06592f));
            isoData[56].Add(new IsotopeInfo(135.90457573, 0.07854f));
            isoData[56].Add(new IsotopeInfo(136.90582714, 0.11232f));
            isoData[56].Add(new IsotopeInfo(137.905247, 0.71698f));
            isoData[57].Add(new IsotopeInfo(137.9071149, 0.0008881f));
            isoData[57].Add(new IsotopeInfo(138.9063563, 0.9991119f));
            isoData[58].Add(new IsotopeInfo(135.90712921, 0.00185f));
            isoData[58].Add(new IsotopeInfo(137.905991, 0.00251f));
            isoData[58].Add(new IsotopeInfo(139.9054431, 0.8845f));
            isoData[58].Add(new IsotopeInfo(141.9092504, 0.11114f));
            isoData[59].Add(new IsotopeInfo(140.9076576, 1f));
            isoData[60].Add(new IsotopeInfo(141.907729, 0.27152f));
            isoData[60].Add(new IsotopeInfo(142.90982, 0.12174f));
            isoData[60].Add(new IsotopeInfo(143.910093, 0.23798f));
            isoData[60].Add(new IsotopeInfo(144.9125793, 0.08293f));
            isoData[60].Add(new IsotopeInfo(145.9131226, 0.17189f));
            isoData[60].Add(new IsotopeInfo(147.9168993, 0.05756f));
            isoData[60].Add(new IsotopeInfo(149.9209022, 0.05638f));
            isoData[61].Add(new IsotopeInfo(144.9127559, 1f));
            isoData[62].Add(new IsotopeInfo(143.9120065, 0.0307f));
            isoData[62].Add(new IsotopeInfo(146.9149044, 0.1499f));
            isoData[62].Add(new IsotopeInfo(147.9148292, 0.1124f));
            isoData[62].Add(new IsotopeInfo(148.9171921, 0.1382f));
            isoData[62].Add(new IsotopeInfo(149.9172829, 0.0738f));
            isoData[62].Add(new IsotopeInfo(151.9197397, 0.2675f));
            isoData[62].Add(new IsotopeInfo(153.9222169, 0.2275f));
            isoData[63].Add(new IsotopeInfo(150.9198578, 0.4781f));
            isoData[63].Add(new IsotopeInfo(152.921238, 0.5219f));
            isoData[64].Add(new IsotopeInfo(151.9197995, 0.002f));
            isoData[64].Add(new IsotopeInfo(153.9208741, 0.0218f));
            isoData[64].Add(new IsotopeInfo(154.9226305, 0.148f));
            isoData[64].Add(new IsotopeInfo(155.9221312, 0.2047f));
            isoData[64].Add(new IsotopeInfo(156.9239686, 0.1565f));
            isoData[64].Add(new IsotopeInfo(157.9241123, 0.2484f));
            isoData[64].Add(new IsotopeInfo(159.9270624, 0.2186f));
            isoData[65].Add(new IsotopeInfo(158.9253547, 1f));
            isoData[66].Add(new IsotopeInfo(155.9242847, 0.00056f));
            isoData[66].Add(new IsotopeInfo(157.9244159, 0.00095f));
            isoData[66].Add(new IsotopeInfo(159.9252046, 0.02329f));
            isoData[66].Add(new IsotopeInfo(160.9269405, 0.18889f));
            isoData[66].Add(new IsotopeInfo(161.9268056, 0.25475f));
            isoData[66].Add(new IsotopeInfo(162.9287383, 0.24896f));
            isoData[66].Add(new IsotopeInfo(163.9291819, 0.2826f));
            isoData[67].Add(new IsotopeInfo(164.9303288, 1f));
            isoData[68].Add(new IsotopeInfo(161.9287884, 0.00139f));
            isoData[68].Add(new IsotopeInfo(163.9292088, 0.01601f));
            isoData[68].Add(new IsotopeInfo(165.9302995, 0.33503f));
            isoData[68].Add(new IsotopeInfo(166.9320546, 0.22869f));
            isoData[68].Add(new IsotopeInfo(167.9323767, 0.26978f));
            isoData[68].Add(new IsotopeInfo(169.9354702, 0.1491f));
            isoData[69].Add(new IsotopeInfo(168.9342179, 1f));
            isoData[70].Add(new IsotopeInfo(167.9338896, 0.00123f));
            isoData[70].Add(new IsotopeInfo(169.9347664, 0.02982f));
            isoData[70].Add(new IsotopeInfo(170.9363302, 0.1409f));
            isoData[70].Add(new IsotopeInfo(171.9363859, 0.2168f));
            isoData[70].Add(new IsotopeInfo(172.9382151, 0.16103f));
            isoData[70].Add(new IsotopeInfo(173.9388664, 0.32026f));
            isoData[70].Add(new IsotopeInfo(175.9425764, 0.12996f));
            isoData[71].Add(new IsotopeInfo(174.9407752, 0.97401f));
            isoData[71].Add(new IsotopeInfo(175.9426897, 0.02599f));
            isoData[72].Add(new IsotopeInfo(173.9400461, 0.0016f));
            isoData[72].Add(new IsotopeInfo(175.9414076, 0.0526f));
            isoData[72].Add(new IsotopeInfo(176.9432277, 0.186f));
            isoData[72].Add(new IsotopeInfo(177.9437058, 0.2728f));
            isoData[72].Add(new IsotopeInfo(178.9458232, 0.1362f));
            isoData[72].Add(new IsotopeInfo(179.946557, 0.3508f));
            isoData[73].Add(new IsotopeInfo(179.9474648, 0.0001201f));
            isoData[73].Add(new IsotopeInfo(180.9479958, 0.9998799f));
            isoData[74].Add(new IsotopeInfo(179.9467108, 0.0012f));
            isoData[74].Add(new IsotopeInfo(181.94820394, 0.265f));
            isoData[74].Add(new IsotopeInfo(182.95022275, 0.1431f));
            isoData[74].Add(new IsotopeInfo(183.95093092, 0.3064f));
            isoData[74].Add(new IsotopeInfo(185.9543628, 0.2843f));
            isoData[75].Add(new IsotopeInfo(184.9529545, 0.374f));
            isoData[75].Add(new IsotopeInfo(186.9557501, 0.626f));
            isoData[76].Add(new IsotopeInfo(183.9524885, 0.0002f));
            isoData[76].Add(new IsotopeInfo(185.953835, 0.0159f));
            isoData[76].Add(new IsotopeInfo(186.9557474, 0.0196f));
            isoData[76].Add(new IsotopeInfo(187.9558352, 0.1324f));
            isoData[76].Add(new IsotopeInfo(188.9581442, 0.1615f));
            isoData[76].Add(new IsotopeInfo(189.9584437, 0.2626f));
            isoData[76].Add(new IsotopeInfo(191.961477, 0.4078f));
            isoData[77].Add(new IsotopeInfo(190.9605893, 0.373f));
            isoData[77].Add(new IsotopeInfo(192.9629216, 0.627f));
            isoData[78].Add(new IsotopeInfo(189.9599297, 0.00012f));
            isoData[78].Add(new IsotopeInfo(191.9610387, 0.00782f));
            isoData[78].Add(new IsotopeInfo(193.9626809, 0.3286f));
            isoData[78].Add(new IsotopeInfo(194.9647917, 0.3378f));
            isoData[78].Add(new IsotopeInfo(195.96495209, 0.2521f));
            isoData[78].Add(new IsotopeInfo(197.9678949, 0.07356f));
            isoData[79].Add(new IsotopeInfo(196.96656879, 1f));
            isoData[80].Add(new IsotopeInfo(195.9658326, 0.0015f));
            isoData[80].Add(new IsotopeInfo(197.9667686, 0.0997f));
            isoData[80].Add(new IsotopeInfo(198.96828064, 0.1687f));
            isoData[80].Add(new IsotopeInfo(199.96832659, 0.231f));
            isoData[80].Add(new IsotopeInfo(200.97030284, 0.1318f));
            isoData[80].Add(new IsotopeInfo(201.9706434, 0.2986f));
            isoData[80].Add(new IsotopeInfo(203.97349398, 0.0687f));
            isoData[81].Add(new IsotopeInfo(202.9723446, 0.2952f));
            isoData[81].Add(new IsotopeInfo(204.9744278, 0.7048f));
            isoData[82].Add(new IsotopeInfo(203.973044, 0.014f));
            isoData[82].Add(new IsotopeInfo(205.9744657, 0.241f));
            isoData[82].Add(new IsotopeInfo(206.9758973, 0.221f));
            isoData[82].Add(new IsotopeInfo(207.9766525, 0.524f));
            isoData[83].Add(new IsotopeInfo(208.9803991, 1f));
            isoData[84].Add(new IsotopeInfo(208.9824308, 1f));
            isoData[85].Add(new IsotopeInfo(209.9871479, 1f));
            isoData[86].Add(new IsotopeInfo(222.0175782, 1f));
            isoData[87].Add(new IsotopeInfo(223.019736, 1f));
            isoData[88].Add(new IsotopeInfo(226.0254103, 1f));
            isoData[89].Add(new IsotopeInfo(227.0277523, 1f));
            isoData[90].Add(new IsotopeInfo(232.0380558, 1f));
            isoData[91].Add(new IsotopeInfo(231.0358842, 1f));
            isoData[92].Add(new IsotopeInfo(234.0409523, 0.000054f));
            isoData[92].Add(new IsotopeInfo(235.0439301, 0.007204f));
            isoData[92].Add(new IsotopeInfo(238.0507884, 0.992742f));
            isoData[93].Add(new IsotopeInfo(237.0481736, 1f));
            isoData[94].Add(new IsotopeInfo(244.0642053, 1f));
            isoData[95].Add(new IsotopeInfo(243.0613813, 1f));
            isoData[96].Add(new IsotopeInfo(247.0703541, 1f));
            isoData[97].Add(new IsotopeInfo(247.0703073, 1f));
            isoData[98].Add(new IsotopeInfo(251.0795886, 1f));
            isoData[99].Add(new IsotopeInfo(252.08298, 1f));
            isoData[100].Add(new IsotopeInfo(257.0951061, 1f));
            isoData[101].Add(new IsotopeInfo(258.0984315, 1f));
            isoData[102].Add(new IsotopeInfo(259.10103, 1f));
            isoData[103].Add(new IsotopeInfo(262.10961, 1f));
            isoData[104].Add(new IsotopeInfo(267.12179, 1f));
            isoData[105].Add(new IsotopeInfo(268.12567, 1f));
            isoData[106].Add(new IsotopeInfo(271.13393, 1f));
            isoData[107].Add(new IsotopeInfo(272.13826, 1f));
            isoData[108].Add(new IsotopeInfo(270.13429, 1f));
            isoData[109].Add(new IsotopeInfo(276.15159, 1f));
            isoData[110].Add(new IsotopeInfo(281.16451, 1f));
            isoData[111].Add(new IsotopeInfo(280.16514, 1f));
            isoData[112].Add(new IsotopeInfo(285.17712, 1f));
            isoData[113].Add(new IsotopeInfo(284.17873, 1f));
            isoData[114].Add(new IsotopeInfo(289.19042, 1f));
            isoData[115].Add(new IsotopeInfo(288.19274, 1f));
            isoData[116].Add(new IsotopeInfo(293.20449, 1f));
            isoData[117].Add(new IsotopeInfo(292.20746, 1f));
            isoData[118].Add(new IsotopeInfo(294.21392, 1f));

            // Note: We stored data in the isoData[] array above
            // then copy to the ElementStats[] array here for the purposes of
            // decreasing the size of this method
            for (var atomicNumber = 1; atomicNumber <= ElementsAndAbbrevs.ELEMENT_COUNT; atomicNumber++)
            {
                var stats = elementStats[atomicNumber];
                short isotopeIndex = 0;

                if (atomicNumber >= isoData.Length)
                    continue;

                foreach (var isotope in isoData[atomicNumber])
                {
                    stats.Isotopes.Add(isotope);
                    isotopeIndex++;
                    if (isotopeIndex > ElementsAndAbbrevs.MAX_ISOTOPES)
                        break;
                }

                stats.Isotopes.Capacity = stats.Isotopes.Count;
            }
        }
    }
}
