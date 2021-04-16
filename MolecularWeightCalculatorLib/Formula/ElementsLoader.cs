using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.Formula
{
    [ComVisible(false)]
    internal static class ElementsLoader
    {
        // Ignore Spelling: csv, txt, frm, isoData

        /// <summary>
        /// Loads the element data and return it.
        /// </summary>
        public static ElementMem[] MemoryLoadElements()
        {
            var elementData = new ElementMem[ElementsAndAbbrevs.ELEMENT_COUNT + 1];

            // Data obtained from the Perma-Chart Science Series periodic table, 1993.
            // For Radioactive elements, the most stable isotope is NOT used;
            // instead, an average molecular weight is used, just like with other elements.

            // Uncertainties from CRC Handbook of Chemistry and Physics, except for
            // Radioactive elements, where uncertainty was estimated to be .n5 where
            // specificElementProperty represents the number digits after the decimal point but before the last
            // number of the molecular weight.

            // For example, for No, MW = 259.1009 (±0.0005)

            // Assigning element names, Charges, integer weight, isotopic weight, average weight, and average weight uncertainty
            elementData[1] = new ElementMem("H", 1, 1, 1.0078246d, 1.00794d, 0.00007d);
            elementData[2] = new ElementMem("He", 0, 4, 4.0026029d, 4.002602d, 0.000002d);
            elementData[3] = new ElementMem("Li", 1, 7, 7.016005d, 6.941d, 0.002d);
            elementData[4] = new ElementMem("Be", 2, 9, 9.012183d, 9.012182d, 0.000003d);
            elementData[5] = new ElementMem("B", 3, 11, 11.009305d, 10.811d, 0.007d);
            elementData[6] = new ElementMem("C", 4, 12, 12d, 12.0107d, 0.0008d);
            elementData[7] = new ElementMem("N", -3, 14, 14.003074d, 14.00674d, 0.00007d);
            elementData[8] = new ElementMem("O", -2, 16, 15.994915d, 15.9994d, 0.0003d);
            elementData[9] = new ElementMem("F", -1, 19, 18.9984032d, 18.9984032d, 0.0000005d);
            elementData[10] = new ElementMem("Ne", 0, 20, 19.992439d, 20.1797d, 0.0006d);
            elementData[11] = new ElementMem("Na", 1, 23, 22.98977d, 22.98977d, 0.000002d);
            elementData[12] = new ElementMem("Mg", 2, 24, 23.98505d, 24.305d, 0.0006d);
            elementData[13] = new ElementMem("Al", 3, 27, 26.981541d, 26.981538d, 0.000002d);
            elementData[14] = new ElementMem("Si", 4, 28, 27.976928d, 28.0855d, 0.0003d);
            elementData[15] = new ElementMem("P", -3, 31, 30.973763d, 30.973761d, 0.000002d);
            elementData[16] = new ElementMem("S", -2, 32, 31.972072d, 32.066d, 0.006d);
            elementData[17] = new ElementMem("Cl", -1, 35, 34.968853d, 35.4527d, 0.0009d);
            elementData[18] = new ElementMem("Ar", 0, 40, 39.962383d, 39.948d, 0.001d);
            elementData[19] = new ElementMem("K", 1, 39, 38.963708d, 39.0983d, 0.0001d);
            elementData[20] = new ElementMem("Ca", 2, 40, 39.962591d, 40.078d, 0.004d);
            elementData[21] = new ElementMem("Sc", 3, 45, 44.955914d, 44.95591d, 0.000008d);
            elementData[22] = new ElementMem("Ti", 4, 48, 47.947947d, 47.867d, 0.001d);
            elementData[23] = new ElementMem("V", 5, 51, 50.943963d, 50.9415d, 0.0001d);
            elementData[24] = new ElementMem("Cr", 3, 52, 51.94051d, 51.9961d, 0.0006d);
            elementData[25] = new ElementMem("Mn", 2, 55, 54.938046d, 54.938049d, 0.000009d);
            elementData[26] = new ElementMem("Fe", 3, 56, 55.934939d, 55.845d, 0.002d);
            elementData[27] = new ElementMem("Co", 2, 59, 58.933198d, 58.9332d, 0.000009d);
            elementData[28] = new ElementMem("Ni", 2, 58, 57.935347d, 58.6934d, 0.0002d);
            elementData[29] = new ElementMem("Cu", 2, 63, 62.929599d, 63.546d, 0.003d);
            elementData[30] = new ElementMem("Zn", 2, 64, 63.929145d, 65.39d, 0.02d);
            elementData[31] = new ElementMem("Ga", 3, 69, 68.925581d, 69.723d, 0.001d);
            elementData[32] = new ElementMem("Ge", 4, 72, 71.92208d, 72.61d, 0.02d);
            elementData[33] = new ElementMem("As", -3, 75, 74.921596d, 74.9216d, 0.00002d);
            elementData[34] = new ElementMem("Se", -2, 80, 79.916521d, 78.96d, 0.03d);
            elementData[35] = new ElementMem("Br", -1, 79, 78.918336d, 79.904d, 0.001d);
            elementData[36] = new ElementMem("Kr", 0, 84, 83.911506d, 83.8d, 0.01d);
            elementData[37] = new ElementMem("Rb", 1, 85, 84.9118d, 85.4678d, 0.0003d);
            elementData[38] = new ElementMem("Sr", 2, 88, 87.905625d, 87.62d, 0.01d);
            elementData[39] = new ElementMem("Y", 3, 89, 88.905856d, 88.90585d, 0.00002d);
            elementData[40] = new ElementMem("Zr", 4, 90, 89.904708d, 91.224d, 0.002d);
            elementData[41] = new ElementMem("Nb", 5, 93, 92.906378d, 92.90638d, 0.00002d);
            elementData[42] = new ElementMem("Mo", 6, 98, 97.905405d, 95.94d, 0.01d);
            elementData[43] = new ElementMem("Tc", 7, 98, 98d, 97.9072d, 0.0005d);
            elementData[44] = new ElementMem("Ru", 4, 102, 101.90434d, 101.07d, 0.02d);
            elementData[45] = new ElementMem("Rh", 3, 103, 102.905503d, 102.9055d, 0.00002d);
            elementData[46] = new ElementMem("Pd", 2, 106, 105.903475d, 106.42d, 0.01d);
            elementData[47] = new ElementMem("Ag", 1, 107, 106.905095d, 107.8682d, 0.0002d);
            elementData[48] = new ElementMem("Cd", 2, 114, 113.903361d, 112.411d, 0.008d);
            elementData[49] = new ElementMem("In", 3, 115, 114.903875d, 114.818d, 0.003d);
            elementData[50] = new ElementMem("Sn", 4, 120, 119.902199d, 118.71d, 0.007d);
            elementData[51] = new ElementMem("Sb", -3, 121, 120.903824d, 121.76d, 0.001d);
            elementData[52] = new ElementMem("Te", -2, 130, 129.906229d, 127.6d, 0.03d);
            elementData[53] = new ElementMem("I", -1, 127, 126.904477d, 126.90447d, 0.00003d);
            elementData[54] = new ElementMem("Xe", 0, 132, 131.904148d, 131.29d, 0.02d);
            elementData[55] = new ElementMem("Cs", 1, 133, 132.905433d, 132.90545d, 0.00002d);
            elementData[56] = new ElementMem("Ba", 2, 138, 137.905236d, 137.327d, 0.007d);
            elementData[57] = new ElementMem("La", 3, 139, 138.906355d, 138.9055d, 0.0002d);
            elementData[58] = new ElementMem("Ce", 3, 140, 139.905442d, 140.116d, 0.001d);
            elementData[59] = new ElementMem("Pr", 4, 141, 140.907657d, 140.90765d, 0.00002d);
            elementData[60] = new ElementMem("Nd", 3, 142, 141.907731d, 144.24d, 0.03d);
            elementData[61] = new ElementMem("Pm", 3, 145, 145d, 144.9127d, 0.0005d);
            elementData[62] = new ElementMem("Sm", 3, 152, 151.919741d, 150.36d, 0.03d);
            elementData[63] = new ElementMem("Eu", 3, 153, 152.921243d, 151.964d, 0.001d);
            elementData[64] = new ElementMem("Gd", 3, 158, 157.924111d, 157.25d, 0.03d);
            elementData[65] = new ElementMem("Tb", 3, 159, 158.92535d, 158.92534d, 0.00002d);
            elementData[66] = new ElementMem("Dy", 3, 164, 163.929183d, 162.5d, 0.03d);
            elementData[67] = new ElementMem("Ho", 3, 165, 164.930332d, 164.93032d, 0.00002d);
            elementData[68] = new ElementMem("Er", 3, 166, 165.930305d, 167.26d, 0.03d);
            elementData[69] = new ElementMem("Tm", 3, 169, 168.934225d, 168.93421d, 0.00002d);
            elementData[70] = new ElementMem("Yb", 3, 174, 173.938873d, 173.04d, 0.03d);
            elementData[71] = new ElementMem("Lu", 3, 175, 174.940785d, 174.967d, 0.001d);
            elementData[72] = new ElementMem("Hf", 4, 180, 179.946561d, 178.49d, 0.02d);
            elementData[73] = new ElementMem("Ta", 5, 181, 180.948014d, 180.9479d, 0.0001d);
            elementData[74] = new ElementMem("W", 6, 184, 183.950953d, 183.84d, 0.01d);
            elementData[75] = new ElementMem("Re", 7, 187, 186.955765d, 186.207d, 0.001d);
            elementData[76] = new ElementMem("Os", 4, 192, 191.960603d, 190.23d, 0.03d);
            elementData[77] = new ElementMem("Ir", 4, 193, 192.962942d, 192.217d, 0.03d);
            elementData[78] = new ElementMem("Pt", 4, 195, 194.964785d, 195.078d, 0.002d);
            elementData[79] = new ElementMem("Au", 3, 197, 196.96656d, 196.96655d, 0.00002d);
            elementData[80] = new ElementMem("Hg", 2, 202, 201.970632d, 200.59d, 0.02d);
            elementData[81] = new ElementMem("Tl", 1, 205, 204.97441d, 204.3833d, 0.0002d);
            elementData[82] = new ElementMem("Pb", 2, 208, 207.976641d, 207.2d, 0.1d);
            elementData[83] = new ElementMem("Bi", 3, 209, 208.980388d, 208.98038d, 0.00002d);
            elementData[84] = new ElementMem("Po", 4, 209, 209d, 208.9824d, 0.0005d);
            elementData[85] = new ElementMem("At", -1, 210, 210d, 209.9871d, 0.0005d);
            elementData[86] = new ElementMem("Rn", 0, 222, 222d, 222.0176d, 0.0005d);
            elementData[87] = new ElementMem("Fr", 1, 223, 223d, 223.0197d, 0.0005d);
            elementData[88] = new ElementMem("Ra", 2, 227, 227d, 226.0254d, 0.0001d);
            elementData[89] = new ElementMem("Ac", 3, 227, 227d, 227.0278d, 0.00001d);
            elementData[90] = new ElementMem("Th", 4, 232, 232.038054d, 232.0381d, 0.0001d);
            elementData[91] = new ElementMem("Pa", 5, 231, 231d, 231.03588d, 0.00002d);
            elementData[92] = new ElementMem("U", 6, 238, 238.050786d, 238.0289d, 0.0001d);
            elementData[93] = new ElementMem("Np", 5, 237, 237d, 237.0482d, 0.0005d);
            elementData[94] = new ElementMem("Pu", 4, 244, 244d, 244.0642d, 0.0005d);
            elementData[95] = new ElementMem("Am", 3, 243, 243d, 243.0614d, 0.0005d);
            elementData[96] = new ElementMem("Cm", 3, 247, 247d, 247.0703d, 0.0005d);
            elementData[97] = new ElementMem("Bk", 3, 247, 247d, 247.0703d, 0.0005d);
            elementData[98] = new ElementMem("Cf", 3, 251, 251d, 251.0796d, 0.0005d);
            elementData[99] = new ElementMem("Es", 3, 252, 252d, 252.083d, 0.005d);
            elementData[100] = new ElementMem("Fm", 3, 257, 257d, 257.0951d, 0.0005d);
            elementData[101] = new ElementMem("Md", 3, 258, 258d, 258.1d, 0.05d);
            elementData[102] = new ElementMem("No", 3, 269, 269d, 259.1009d, 0.0005d);
            elementData[103] = new ElementMem("Lr", 3, 260, 260d, 262.11d, 0.05d);

            // Unused elements
            // data 104,Unq,Unnilquadium,261.11,.05, 105,Unp,Unnilpentium,262.114,005, 106,Unh,Unnilhexium,263.118,.005, 107,Uns,Unnilseptium,262.12,.05

            return elementData;
        }

        /// <summary>
        /// Stores isotope information in elementStats[]
        /// </summary>
        internal static void MemoryLoadIsotopes(ElementInfo[] elementStats)
        {
            // The isoData[] array holds the list of isotopes for each element
            // Create the lists now, to simplify the code below.
            var isoData = new List<IsotopeInfo>[104];
            for (var i = 0; i < 104; i++)
            {
                isoData[i] = new List<IsotopeInfo>(1);
            }

            isoData[1].Add(new IsotopeInfo(1.0078246d, 0.99985f));
            isoData[1].Add(new IsotopeInfo(2.0141018d, 0.00015f));
            isoData[2].Add(new IsotopeInfo(3.01603d, 0.00000137f));
            isoData[2].Add(new IsotopeInfo(4.0026029d, 0.99999863f));
            isoData[3].Add(new IsotopeInfo(6.01512d, 0.0759f));
            isoData[3].Add(new IsotopeInfo(7.016005d, 0.9241f));
            isoData[4].Add(new IsotopeInfo(9.012183d, 1f));
            isoData[5].Add(new IsotopeInfo(10.0129d, 0.199f));
            isoData[5].Add(new IsotopeInfo(11.009305d, 0.801f));
            isoData[6].Add(new IsotopeInfo(12d, 0.9893f));
            isoData[6].Add(new IsotopeInfo(13.00335d, 0.0107f));
            isoData[7].Add(new IsotopeInfo(14.003074d, 0.99632f));
            isoData[7].Add(new IsotopeInfo(15.00011d, 0.00368f));
            isoData[8].Add(new IsotopeInfo(15.994915d, 0.99757f));
            isoData[8].Add(new IsotopeInfo(16.999131d, 0.00038f));
            isoData[8].Add(new IsotopeInfo(17.99916d, 0.00205f));
            isoData[9].Add(new IsotopeInfo(18.9984032d, 1f));
            isoData[10].Add(new IsotopeInfo(19.992439d, 0.9048f));
            isoData[10].Add(new IsotopeInfo(20.99395d, 0.0027f));
            isoData[10].Add(new IsotopeInfo(21.99138d, 0.0925f));
            isoData[11].Add(new IsotopeInfo(22.98977d, 1f));
            isoData[12].Add(new IsotopeInfo(23.98505d, 0.7899f));
            isoData[12].Add(new IsotopeInfo(24.98584d, 0.1f));
            isoData[12].Add(new IsotopeInfo(25.98259d, 0.1101f));
            isoData[13].Add(new IsotopeInfo(26.981541d, 1f));
            isoData[14].Add(new IsotopeInfo(27.976928d, 0.922297f));
            isoData[14].Add(new IsotopeInfo(28.97649d, 0.046832f));
            isoData[14].Add(new IsotopeInfo(29.97376d, 0.030871f));
            isoData[15].Add(new IsotopeInfo(30.973763d, 1f));
            isoData[16].Add(new IsotopeInfo(31.972072d, 0.9493f));
            isoData[16].Add(new IsotopeInfo(32.97146d, 0.0076f));
            isoData[16].Add(new IsotopeInfo(33.96786d, 0.0429f));
            isoData[16].Add(new IsotopeInfo(35.96709d, 0.0002f));
            isoData[17].Add(new IsotopeInfo(34.968853d, 0.7578f));
            isoData[17].Add(new IsotopeInfo(36.99999d, 0.2422f));
            isoData[18].Add(new IsotopeInfo(35.96755d, 0.003365f));
            isoData[18].Add(new IsotopeInfo(37.96272d, 0.000632f));
            isoData[18].Add(new IsotopeInfo(39.96999d, 0.996003f)); // Note: Alternate mass is 39.962383
            isoData[19].Add(new IsotopeInfo(38.963708d, 0.932581f));
            isoData[19].Add(new IsotopeInfo(39.963999d, 0.000117f));
            isoData[19].Add(new IsotopeInfo(40.961825d, 0.067302f));
            isoData[20].Add(new IsotopeInfo(39.962591d, 0.96941f));
            isoData[20].Add(new IsotopeInfo(41.958618d, 0.00647f));
            isoData[20].Add(new IsotopeInfo(42.958766d, 0.00135f));
            isoData[20].Add(new IsotopeInfo(43.95548d, 0.02086f));
            isoData[20].Add(new IsotopeInfo(45.953689d, 0.00004f));
            isoData[20].Add(new IsotopeInfo(47.952533d, 0.00187f));
            isoData[21].Add(new IsotopeInfo(44.959404d, 1f)); // Note: Alternate mass is 44.955914
            isoData[22].Add(new IsotopeInfo(45.952629d, 0.0825f));
            isoData[22].Add(new IsotopeInfo(46.951764d, 0.0744f));
            isoData[22].Add(new IsotopeInfo(47.947947d, 0.7372f));
            isoData[22].Add(new IsotopeInfo(48.947871d, 0.0541f));
            isoData[22].Add(new IsotopeInfo(49.944792d, 0.0518f));
            isoData[23].Add(new IsotopeInfo(49.947161d, 0.0025f));
            isoData[23].Add(new IsotopeInfo(50.943963d, 0.9975f));
            isoData[24].Add(new IsotopeInfo(49.946046d, 0.04345f));
            isoData[24].Add(new IsotopeInfo(51.940509d, 0.83789f));
            isoData[24].Add(new IsotopeInfo(52.940651d, 0.09501f));
            isoData[24].Add(new IsotopeInfo(53.938882d, 0.02365f));
            isoData[25].Add(new IsotopeInfo(54.938046d, 1f));
            isoData[26].Add(new IsotopeInfo(53.939612d, 0.05845f));
            isoData[26].Add(new IsotopeInfo(55.934939d, 0.91754f));
            isoData[26].Add(new IsotopeInfo(56.935396d, 0.02119f));
            isoData[26].Add(new IsotopeInfo(57.933277d, 0.00282f));
            isoData[27].Add(new IsotopeInfo(58.933198d, 1f));
            isoData[28].Add(new IsotopeInfo(57.935347d, 0.680769f));
            isoData[28].Add(new IsotopeInfo(59.930788d, 0.262231f));
            isoData[28].Add(new IsotopeInfo(60.931058d, 0.011399f));
            isoData[28].Add(new IsotopeInfo(61.928346d, 0.036345f));
            isoData[28].Add(new IsotopeInfo(63.927968d, 0.009256f));
            isoData[29].Add(new IsotopeInfo(62.939598d, 0.6917f)); // Note: Alternate mass is 62.929599
            isoData[29].Add(new IsotopeInfo(64.927793d, 0.3083f));
            isoData[30].Add(new IsotopeInfo(63.929145d, 0.4863f));
            isoData[30].Add(new IsotopeInfo(65.926034d, 0.279f));
            isoData[30].Add(new IsotopeInfo(66.927129d, 0.041f));
            isoData[30].Add(new IsotopeInfo(67.924846d, 0.1875f));
            isoData[30].Add(new IsotopeInfo(69.925325d, 0.0062f));
            isoData[31].Add(new IsotopeInfo(68.925581d, 0.60108f));
            isoData[31].Add(new IsotopeInfo(70.9247d, 0.39892f));
            isoData[32].Add(new IsotopeInfo(69.92425d, 0.2084f));
            isoData[32].Add(new IsotopeInfo(71.922079d, 0.2754f));
            isoData[32].Add(new IsotopeInfo(72.923463d, 0.0773f));
            isoData[32].Add(new IsotopeInfo(73.921177d, 0.3628f));
            isoData[32].Add(new IsotopeInfo(75.921401d, 0.0761f));
            isoData[33].Add(new IsotopeInfo(74.921596d, 1f));
            isoData[34].Add(new IsotopeInfo(73.922475d, 0.0089f));
            isoData[34].Add(new IsotopeInfo(75.919212d, 0.0937f));
            isoData[34].Add(new IsotopeInfo(76.919912d, 0.0763f));
            isoData[34].Add(new IsotopeInfo(77.919d, 0.2377f));
            isoData[34].Add(new IsotopeInfo(79.916521d, 0.4961f));
            isoData[34].Add(new IsotopeInfo(81.916698d, 0.0873f));
            isoData[35].Add(new IsotopeInfo(78.918336d, 0.5069f));
            isoData[35].Add(new IsotopeInfo(80.916289d, 0.4931f));
            isoData[36].Add(new IsotopeInfo(77.92d, 0.0035f));
            isoData[36].Add(new IsotopeInfo(79.91638d, 0.0228f));
            isoData[36].Add(new IsotopeInfo(81.913482d, 0.1158f));
            isoData[36].Add(new IsotopeInfo(82.914135d, 0.1149f));
            isoData[36].Add(new IsotopeInfo(83.911506d, 0.57f));
            isoData[36].Add(new IsotopeInfo(85.910616d, 0.173f));
            isoData[37].Add(new IsotopeInfo(84.911794d, 0.7217f));
            isoData[37].Add(new IsotopeInfo(86.909187d, 0.2783f));
            isoData[38].Add(new IsotopeInfo(83.91343d, 0.0056f));
            isoData[38].Add(new IsotopeInfo(85.909267d, 0.0986f));
            isoData[38].Add(new IsotopeInfo(86.908884d, 0.07f));
            isoData[38].Add(new IsotopeInfo(87.905625d, 0.8258f));
            isoData[39].Add(new IsotopeInfo(88.905856d, 1f));
            isoData[40].Add(new IsotopeInfo(89.904708d, 0.5145f));
            isoData[40].Add(new IsotopeInfo(90.905644d, 0.1122f));
            isoData[40].Add(new IsotopeInfo(91.905039d, 0.1715f));
            isoData[40].Add(new IsotopeInfo(93.906314d, 0.1738f));
            isoData[40].Add(new IsotopeInfo(95.908275d, 0.028f));
            isoData[41].Add(new IsotopeInfo(92.906378d, 1f));
            isoData[42].Add(new IsotopeInfo(91.906808d, 0.1484f));
            isoData[42].Add(new IsotopeInfo(93.905085d, 0.0925f));
            isoData[42].Add(new IsotopeInfo(94.90584d, 0.1592f));
            isoData[42].Add(new IsotopeInfo(95.904678d, 0.1668f));
            isoData[42].Add(new IsotopeInfo(96.90602d, 0.0955f));
            isoData[42].Add(new IsotopeInfo(97.905405d, 0.2413f));
            isoData[42].Add(new IsotopeInfo(99.907477d, 0.0963f));
            isoData[43].Add(new IsotopeInfo(97.9072d, 1f));
            isoData[44].Add(new IsotopeInfo(95.907599d, 0.0554f));
            isoData[44].Add(new IsotopeInfo(97.905287d, 0.0187f));
            isoData[44].Add(new IsotopeInfo(98.905939d, 0.1276f));
            isoData[44].Add(new IsotopeInfo(99.904219d, 0.126f));
            isoData[44].Add(new IsotopeInfo(100.905582d, 0.1706f));
            isoData[44].Add(new IsotopeInfo(101.904348d, 0.3155f));
            isoData[44].Add(new IsotopeInfo(103.905424d, 0.1862f));
            isoData[45].Add(new IsotopeInfo(102.905503d, 1f));
            isoData[46].Add(new IsotopeInfo(101.905634d, 0.0102f));
            isoData[46].Add(new IsotopeInfo(103.904029d, 0.1114f));
            isoData[46].Add(new IsotopeInfo(104.905079d, 0.2233f));
            isoData[46].Add(new IsotopeInfo(105.903475d, 0.2733f));
            isoData[46].Add(new IsotopeInfo(107.903895d, 0.2646f));
            isoData[46].Add(new IsotopeInfo(109.905167d, 0.1172f));
            isoData[47].Add(new IsotopeInfo(106.905095d, 0.51839f));
            isoData[47].Add(new IsotopeInfo(108.904757d, 0.48161f));
            isoData[48].Add(new IsotopeInfo(105.906461d, 0.0125f));
            isoData[48].Add(new IsotopeInfo(107.904176d, 0.0089f));
            isoData[48].Add(new IsotopeInfo(109.903005d, 0.1249f));
            isoData[48].Add(new IsotopeInfo(110.904182d, 0.128f));
            isoData[48].Add(new IsotopeInfo(111.902758d, 0.2413f));
            isoData[48].Add(new IsotopeInfo(112.9044d, 0.1222f));
            isoData[48].Add(new IsotopeInfo(113.903361d, 0.2873f));
            isoData[48].Add(new IsotopeInfo(115.904754d, 0.0749f));
            isoData[49].Add(new IsotopeInfo(112.904061d, 0.0429f));
            isoData[49].Add(new IsotopeInfo(114.903875d, 0.9571f));
            isoData[50].Add(new IsotopeInfo(111.904826d, 0.0097f));
            isoData[50].Add(new IsotopeInfo(113.902784d, 0.0066f));
            isoData[50].Add(new IsotopeInfo(114.903348d, 0.0034f));
            isoData[50].Add(new IsotopeInfo(115.901747d, 0.1454f));
            isoData[50].Add(new IsotopeInfo(116.902956d, 0.0768f));
            isoData[50].Add(new IsotopeInfo(117.901609d, 0.2422f));
            isoData[50].Add(new IsotopeInfo(118.90331d, 0.0859f));
            isoData[50].Add(new IsotopeInfo(119.902199d, 0.3258f));
            isoData[50].Add(new IsotopeInfo(121.90344d, 0.0463f));
            isoData[50].Add(new IsotopeInfo(123.905274d, 0.0579f));
            isoData[51].Add(new IsotopeInfo(120.903824d, 0.5721f));
            isoData[51].Add(new IsotopeInfo(122.904216d, 0.4279f));
            isoData[52].Add(new IsotopeInfo(119.904048d, 0.0009f));
            isoData[52].Add(new IsotopeInfo(121.903054d, 0.0255f));
            isoData[52].Add(new IsotopeInfo(122.904271d, 0.0089f));
            isoData[52].Add(new IsotopeInfo(123.902823d, 0.0474f));
            isoData[52].Add(new IsotopeInfo(124.904433d, 0.0707f));
            isoData[52].Add(new IsotopeInfo(125.903314d, 0.1884f));
            isoData[52].Add(new IsotopeInfo(127.904463d, 0.3174f));
            isoData[52].Add(new IsotopeInfo(129.906229d, 0.3408f));
            isoData[53].Add(new IsotopeInfo(126.904477d, 1f));
            isoData[54].Add(new IsotopeInfo(123.905894d, 0.0009f));
            isoData[54].Add(new IsotopeInfo(125.904281d, 0.0009f));
            isoData[54].Add(new IsotopeInfo(127.903531d, 0.0192f));
            isoData[54].Add(new IsotopeInfo(128.90478d, 0.2644f));
            isoData[54].Add(new IsotopeInfo(129.903509d, 0.0408f));
            isoData[54].Add(new IsotopeInfo(130.905072d, 0.2118f));
            isoData[54].Add(new IsotopeInfo(131.904148d, 0.2689f));
            isoData[54].Add(new IsotopeInfo(133.905395d, 0.1044f));
            isoData[54].Add(new IsotopeInfo(135.907214d, 0.0887f));
            isoData[55].Add(new IsotopeInfo(132.905433d, 1f));
            isoData[56].Add(new IsotopeInfo(129.906282d, 0.00106f));
            isoData[56].Add(new IsotopeInfo(131.905042d, 0.00101f));
            isoData[56].Add(new IsotopeInfo(133.904486d, 0.02417f));
            isoData[56].Add(new IsotopeInfo(134.905665d, 0.06592f));
            isoData[56].Add(new IsotopeInfo(135.904553d, 0.07854f));
            isoData[56].Add(new IsotopeInfo(136.905812d, 0.11232f));
            isoData[56].Add(new IsotopeInfo(137.905236d, 0.71698f));
            isoData[57].Add(new IsotopeInfo(137.907105d, 0.0009f));
            isoData[57].Add(new IsotopeInfo(138.906355d, 0.9991f));
            isoData[58].Add(new IsotopeInfo(135.90714d, 0.00185f));
            isoData[58].Add(new IsotopeInfo(137.905985d, 0.00251f));
            isoData[58].Add(new IsotopeInfo(139.905442d, 0.8845f));
            isoData[58].Add(new IsotopeInfo(141.909241d, 0.11114f));
            isoData[59].Add(new IsotopeInfo(140.907657d, 1f));
            isoData[60].Add(new IsotopeInfo(141.907731d, 0.272f));
            isoData[60].Add(new IsotopeInfo(142.90981d, 0.122f));
            isoData[60].Add(new IsotopeInfo(143.910083d, 0.238f));
            isoData[60].Add(new IsotopeInfo(144.91257d, 0.083f));
            isoData[60].Add(new IsotopeInfo(145.913113d, 0.172f));
            isoData[60].Add(new IsotopeInfo(147.916889d, 0.057f));
            isoData[60].Add(new IsotopeInfo(149.920887d, 0.056f));
            isoData[61].Add(new IsotopeInfo(144.9127d, 1f));
            isoData[62].Add(new IsotopeInfo(143.911998d, 0.0307f));
            isoData[62].Add(new IsotopeInfo(146.914895d, 0.1499f));
            isoData[62].Add(new IsotopeInfo(147.91482d, 0.1124f));
            isoData[62].Add(new IsotopeInfo(148.917181d, 0.1382f));
            isoData[62].Add(new IsotopeInfo(149.917273d, 0.0738f));
            isoData[62].Add(new IsotopeInfo(151.919741d, 0.2675f));
            isoData[62].Add(new IsotopeInfo(153.922206d, 0.2275f));
            isoData[63].Add(new IsotopeInfo(150.919847d, 0.4781f));
            isoData[63].Add(new IsotopeInfo(152.921243d, 0.5219f));
            isoData[64].Add(new IsotopeInfo(151.919786d, 0.002f));
            isoData[64].Add(new IsotopeInfo(153.920861d, 0.0218f));
            isoData[64].Add(new IsotopeInfo(154.922618d, 0.148f));
            isoData[64].Add(new IsotopeInfo(155.922118d, 0.2047f));
            isoData[64].Add(new IsotopeInfo(156.923956d, 0.1565f));
            isoData[64].Add(new IsotopeInfo(157.924111d, 0.2484f));
            isoData[64].Add(new IsotopeInfo(159.927049d, 0.2186f));
            isoData[65].Add(new IsotopeInfo(158.92535d, 1f));
            isoData[66].Add(new IsotopeInfo(155.925277d, 0.0006f));
            isoData[66].Add(new IsotopeInfo(157.924403d, 0.001f));
            isoData[66].Add(new IsotopeInfo(159.925193d, 0.0234f));
            isoData[66].Add(new IsotopeInfo(160.92693d, 0.1891f));
            isoData[66].Add(new IsotopeInfo(161.926795d, 0.2551f));
            isoData[66].Add(new IsotopeInfo(162.928728d, 0.249f));
            isoData[66].Add(new IsotopeInfo(163.929183d, 0.2818f));
            isoData[67].Add(new IsotopeInfo(164.930332d, 1f));
            isoData[68].Add(new IsotopeInfo(161.928775d, 0.0014f));
            isoData[68].Add(new IsotopeInfo(163.929198d, 0.0161f));
            isoData[68].Add(new IsotopeInfo(165.930305d, 0.3361f));
            isoData[68].Add(new IsotopeInfo(166.932046d, 0.2293f));
            isoData[68].Add(new IsotopeInfo(167.932368d, 0.2678f));
            isoData[68].Add(new IsotopeInfo(169.935461d, 0.1493f));
            isoData[69].Add(new IsotopeInfo(168.934225d, 1f));
            isoData[70].Add(new IsotopeInfo(167.932873d, 0.0013f));
            isoData[70].Add(new IsotopeInfo(169.934759d, 0.0304f));
            isoData[70].Add(new IsotopeInfo(170.936323d, 0.1428f));
            isoData[70].Add(new IsotopeInfo(171.936387d, 0.2183f));
            isoData[70].Add(new IsotopeInfo(172.938208d, 0.1613f));
            isoData[70].Add(new IsotopeInfo(173.938873d, 0.3183f));
            isoData[70].Add(new IsotopeInfo(175.942564d, 0.1276f));
            isoData[71].Add(new IsotopeInfo(174.940785d, 0.9741f));
            isoData[71].Add(new IsotopeInfo(175.942679d, 0.0259f));
            isoData[72].Add(new IsotopeInfo(173.94004d, 0.0016f));
            isoData[72].Add(new IsotopeInfo(175.941406d, 0.0526f));
            isoData[72].Add(new IsotopeInfo(176.943217d, 0.186f));
            isoData[72].Add(new IsotopeInfo(177.943696d, 0.2728f));
            isoData[72].Add(new IsotopeInfo(178.945812d, 0.1362f));
            isoData[72].Add(new IsotopeInfo(179.946561d, 0.3508f));
            isoData[73].Add(new IsotopeInfo(179.947462d, 0.00012f));
            isoData[73].Add(new IsotopeInfo(180.948014d, 0.99988f));
            isoData[74].Add(new IsotopeInfo(179.946701d, 0.0012f));
            isoData[74].Add(new IsotopeInfo(181.948202d, 0.265f));
            isoData[74].Add(new IsotopeInfo(182.95022d, 0.1431f));
            isoData[74].Add(new IsotopeInfo(183.950953d, 0.3064f));
            isoData[74].Add(new IsotopeInfo(185.954357d, 0.2843f));
            isoData[75].Add(new IsotopeInfo(184.952951d, 0.374f));
            isoData[75].Add(new IsotopeInfo(186.955765d, 0.626f));
            isoData[76].Add(new IsotopeInfo(183.952488d, 0.0002f));
            isoData[76].Add(new IsotopeInfo(185.95383d, 0.0159f));
            isoData[76].Add(new IsotopeInfo(186.955741d, 0.0196f));
            isoData[76].Add(new IsotopeInfo(187.95586d, 0.1324f));
            isoData[76].Add(new IsotopeInfo(188.958137d, 0.1615f));
            isoData[76].Add(new IsotopeInfo(189.958436d, 0.2626f));
            isoData[76].Add(new IsotopeInfo(191.961467d, 0.4078f)); // Note: Alternate mass is 191.960603
            isoData[77].Add(new IsotopeInfo(190.960584d, 0.373f));
            isoData[77].Add(new IsotopeInfo(192.962942d, 0.627f));
            isoData[78].Add(new IsotopeInfo(189.959917d, 0.00014f));
            isoData[78].Add(new IsotopeInfo(191.961019d, 0.00782f));
            isoData[78].Add(new IsotopeInfo(193.962655d, 0.32967f));
            isoData[78].Add(new IsotopeInfo(194.964785d, 0.33832f));
            isoData[78].Add(new IsotopeInfo(195.964926d, 0.25242f));
            isoData[78].Add(new IsotopeInfo(197.967869d, 0.07163f));
            isoData[79].Add(new IsotopeInfo(196.966543d, 1f));
            isoData[80].Add(new IsotopeInfo(195.965807d, 0.0015f));
            isoData[80].Add(new IsotopeInfo(197.966743d, 0.0997f));
            isoData[80].Add(new IsotopeInfo(198.968254d, 0.1687f));
            isoData[80].Add(new IsotopeInfo(199.9683d, 0.231f));
            isoData[80].Add(new IsotopeInfo(200.970277d, 0.1318f));
            isoData[80].Add(new IsotopeInfo(201.970632d, 0.2986f));
            isoData[80].Add(new IsotopeInfo(203.973467d, 0.0687f));
            isoData[81].Add(new IsotopeInfo(202.97232d, 0.29524f));
            isoData[81].Add(new IsotopeInfo(204.974401d, 0.70476f));
            isoData[82].Add(new IsotopeInfo(203.97302d, 0.014f));
            isoData[82].Add(new IsotopeInfo(205.97444d, 0.241f));
            isoData[82].Add(new IsotopeInfo(206.975872d, 0.221f));
            isoData[82].Add(new IsotopeInfo(207.976641d, 0.524f));
            isoData[83].Add(new IsotopeInfo(208.980388d, 1f));
            isoData[84].Add(new IsotopeInfo(209d, 1f));
            isoData[85].Add(new IsotopeInfo(210d, 1f));
            isoData[86].Add(new IsotopeInfo(222d, 1f));
            isoData[87].Add(new IsotopeInfo(223d, 1f));
            isoData[88].Add(new IsotopeInfo(226d, 1f));
            isoData[89].Add(new IsotopeInfo(227d, 1f));
            isoData[90].Add(new IsotopeInfo(232.038054d, 1f));
            isoData[91].Add(new IsotopeInfo(231d, 1f));
            isoData[92].Add(new IsotopeInfo(234.041637d, 0.000055f));
            isoData[92].Add(new IsotopeInfo(235.043924d, 0.0072f));
            isoData[92].Add(new IsotopeInfo(238.050786d, 0.992745f));
            isoData[93].Add(new IsotopeInfo(237d, 1f));
            isoData[94].Add(new IsotopeInfo(244d, 1f));
            isoData[95].Add(new IsotopeInfo(243d, 1f));
            isoData[96].Add(new IsotopeInfo(247d, 1f));
            isoData[97].Add(new IsotopeInfo(247d, 1f));
            isoData[98].Add(new IsotopeInfo(251d, 1f));
            isoData[99].Add(new IsotopeInfo(252d, 1f));
            isoData[100].Add(new IsotopeInfo(257d, 1f));
            isoData[101].Add(new IsotopeInfo(258d, 1f));
            isoData[102].Add(new IsotopeInfo(259d, 1f));
            isoData[103].Add(new IsotopeInfo(262d, 1f));

            // Note: We stored data in the isoData[] array above
            // then copy to the ElementStats[] array here for the purposes of
            // decreasing the size of this method
            for (var elementNumber = 1; elementNumber <= ElementsAndAbbrevs.ELEMENT_COUNT; elementNumber++)
            {
                var stats = elementStats[elementNumber];
                short isotopeIndex = 0;
                foreach (var isotope in isoData[elementNumber])
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
