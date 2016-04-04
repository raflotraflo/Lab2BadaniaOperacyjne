using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Msagl.Drawing;
using System.Collections.ObjectModel;

namespace Lab2BadaniaOperacyjne
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int kruskalIterationCount = 0;
        int kruskalCost = 0;
        TimeSpan kruskalTime;
        TimeSpan kruskalTime1000x;

        int primIterationCount = 0;
        int primCost = 0;
        TimeSpan primTime;
        TimeSpan primTime1000x;

        DateTime startTime;
        DateTime stopTime;

        public MainWindow()
        {
            InitializeComponent();

            int[,] outMatrix;
            //int[,] simpleMatrix = new int[,] {
            //    { 0, 1, 1, 1, 1 },
            //    { 0, 1, 2, 1, 1 },
            //    { 0, 1, 1, 3, 1 },
            //    { 5, 0, 1, 1, 4 },
            //    { 6, 0, 0, 0, 0 }
            //};

            int[,] simpleMatrix = new int[,] {
                { 0, 5, 5, 1, 1, 1, 5 },
                { 0, 4, 2, 1, 1, 3, 4 },
                { 0, 3, 1, 3, 5, 5, 8 },
                { 5, 0, 5, 1, 4, 6, 6 },
                { 5, 0, 5, 1, 4, 7, 6 },
                { 5, 0, 5, 1, 4, 8, 9 },
                { 6, 0, 0, 0, 0, 7, 5 }
            };

            // Kruskal:

            startTime = DateTime.Now;
            outMatrix = KruskalsAlgorithm(simpleMatrix);
            stopTime = DateTime.Now;

            kruskalTime = stopTime - startTime; //Czas dla pojedyńczego wyowałania algorytmu


            startTime = DateTime.Now;
            for (int i = 0; i < 1000; i++)
            {
                outMatrix = KruskalsAlgorithm(simpleMatrix); //outMatrix - macierz wyjsciowa
            }
            stopTime = DateTime.Now;
            kruskalTime1000x = stopTime - startTime; //Czas dla tysiąca wywołań algorytmu

            kruskalCost = CheckCost(outMatrix); //Koszt ścieżek



            // Prim:

            startTime = DateTime.Now;
            outMatrix = PrimsAlgorithm(simpleMatrix);
            stopTime = DateTime.Now;

            primTime = stopTime - startTime; //Czas dla pojedyńczego wyowałania algorytmu


            startTime = DateTime.Now;
            for (int i = 0; i < 1000; i++)
            {
                outMatrix = PrimsAlgorithm(simpleMatrix); //outMatrix - macierz wyjsciowa
            }
            stopTime = DateTime.Now;
            primTime1000x = stopTime - startTime; //Czas dla tysiąca wywołań algorytmu

            primCost = CheckCost(outMatrix); //Koszt ścieżek


            //CreateGraph(simpleMatrix);
            CreateGraph(simpleMatrix, outMatrix, true);

            kruskal1xLabel.Content = kruskalTime.Milliseconds + " ms";
            kruskal1000xLabel.Content = kruskalTime1000x.Milliseconds + " ms";
            kruskalCostLabel.Content = kruskalCost.ToString();
            kruskalIterationLabel.Content = kruskalIterationCount.ToString();

            prim1xLabel.Content = primTime.Milliseconds + " ms";
            prim1000xLabel.Content = primTime1000x.Milliseconds + " ms";
            primCostLabel.Content = primCost.ToString();
            primIterationLabel.Content = primIterationCount.ToString();
        }

        private int[,] KruskalsAlgorithm(int[,] matrix)
        {

            int rowsOfmatrix = matrix.GetLength(0);
            int columnsOfMatrix = matrix.GetLength(1);
            //sprawdzenie wymiarów macierzy, w rzeczywistości wystarczy sprawdzić tylko jedną wartość bo macierz jest kwadratowa

            int[,] outMatrix = new int[rowsOfmatrix, columnsOfMatrix];
            //zbiór wierzchołków oryginalnego grafu – każdy wierzchołek jest na początku osobnym drzewem.

            ObservableCollection<KEdge> edges = new ObservableCollection<KEdge>();
            // lista (tablica) zawierająca wszystkie krawędzie
            // KEdge typ obiektu zawierający początek i koniec gałęzi oraz koszt

            ObservableCollection<ObservableCollection<int>> trees = new ObservableCollection<ObservableCollection<int>>();
            // lista zawierające wszystkie wierzchołki jako osobne drzewa

            for (int m = 0; m < columnsOfMatrix; m++)
            {
                for (int n = 0; n < rowsOfmatrix; n++)
                {
                    if (matrix[m, n] != 0)
                    {
                        edges.Add(new KEdge(m, matrix[m, n], n));
                        // przegląd całej macierzy w celu zdefiniowania wszystkich krawędzi 
                    }
                }
                var tree = new ObservableCollection<int>();
                tree.Add(m);
                trees.Add(tree); //dodanie wierzchołków jako osobne drzewa
            }

            var temp1 = edges.OrderBy(e => e.Value);
            edges = new ObservableCollection<KEdge>(temp1);
            // segregacja gałęzi po koszcie

            int iterationCount = 0; //liczba iteracji w danym algorytmie
            while (true)
            {
                iterationCount++;
                if (edges.Count == 0) //jeśli lista krawędzi jest pusta algorytm kończy się
                    break;

                KEdge tempEdge = edges.ElementAt(0); // pobieranie krawędzi o najniższym koszcie
                edges.RemoveAt(0); // usunięcie tej krawędzi z listy

                if (IsDifferentTree(tempEdge, trees)) // jeśli krawędź łaczy dwa różne drzewa to jest ona dodawana do macierzy
                {
                    outMatrix[tempEdge.Source, tempEdge.Target] = tempEdge.Value;
                    CombineTrees(tempEdge, trees);
                }


                if (IsSpanningTree(outMatrix, rowsOfmatrix)) //jeśli drzewo jest rozpinające się algirytm kończy się
                    break;
            }

            kruskalIterationCount = iterationCount;

            //Po zakończeniu algorytmu outMatrix jest minimalnym drzewem rozpinającym.
            return outMatrix;

        }

        private bool IsDifferentTree(KEdge edge, ObservableCollection<ObservableCollection<int>> drzewa)
        {
            bool isDiffrentTree = true;
            bool isFoundNode = false;

            foreach (var drzewo in drzewa)
            {
                foreach (var node in drzewo)
                {
                    if (node == edge.Target)
                        isFoundNode = true;
                }

                if (isFoundNode)
                {
                    foreach (var node in drzewo)
                    {
                        if (node == edge.Source)
                            isDiffrentTree = false;
                    }
                }
            }

            return isDiffrentTree;
        }

        private ObservableCollection<ObservableCollection<int>> CombineTrees(KEdge edge, ObservableCollection<ObservableCollection<int>> trees)
        {

            ObservableCollection<int> sourceTree = new ObservableCollection<int>();
            ObservableCollection<int> targetTree = new ObservableCollection<int>();

            foreach (var tree in trees)
            {
                foreach (var node in tree)
                {
                    if (node == edge.Source)
                        sourceTree = tree;

                    if (node == edge.Target)
                        targetTree = tree;
                }
            }

            foreach (int t in targetTree)
            {
                sourceTree.Add(t);
            }

            trees.Remove(targetTree);

            return trees;
        }

        private bool IsSpanningTree(int[,] matrix, int count)
        {
            int actualnyCount = 0;
            for (int m = 0; m < count; m++)
            {
                for (int n = 0; n < count; n++)
                {
                    if (matrix[m, n] != 0)
                    {
                        actualnyCount++;
                    }
                }
            }

            return (actualnyCount == count - 1);
        }

        private int[,] PrimsAlgorithm(int[,] matrix)
        {

            int rowsOfmatrix = matrix.GetLength(0);
            int columnsOfMatrix = matrix.GetLength(1);
            //sprawdzenie wymiarów macierzy, w rzeczywistości wystarczy sprawdzić tylko jedną wartość bo macierz jest kwadratowa

            int[,] outMatrix = new int[rowsOfmatrix, columnsOfMatrix];


            ObservableCollection<KEdge> edges = new ObservableCollection<KEdge>(); // lista krawędzi
            ObservableCollection<int> nodes = new ObservableCollection<int>(); // lista wierzchołków dodanych do drzewa

            int iterationCount = 0; //liczba iteracji w danym algorytmie

            int m = 0; //startowy wierzchołek
            nodes.Add(m); //dodanie do drzewa startowego wierzchołka

            while (true)
            {
                iterationCount++;

                for (int n = 0; n < rowsOfmatrix; n++)
                {
                    if (matrix[m, n] != 0)
                    {
                        edges.Add(new KEdge(m, matrix[m, n], n));
                        //dodanie krawędzi wychodzących z obecnego drzewa                
                    }
                }

                var temp1 = edges.OrderBy(e => e.Value); //segregacja gałezi po koszcie
                edges = new ObservableCollection<KEdge>(temp1);

                foreach (var e in edges) // przęglad gałęzi począwszy od najtańszej
                {
                    if (nodes.Contains(e.Target))
                    {
                        //edges.Remove(e);
                    }
                    else
                    {
                        //jesli gałęź łaczy nowy wierzchołek z drzewem to jest ona dodawana
                        m = e.Target;
                        nodes.Add(m);
                        outMatrix[e.Source, e.Target] = e.Value;
                        break;
                    }
                }

                //jeśli drzewo jest rozpinające lub brak gałęzi algorytm kończy się
                if (IsSpanningTree(outMatrix, rowsOfmatrix) || nodes.Count == rowsOfmatrix)
                    break;
            }

            primIterationCount = iterationCount;
            return outMatrix;

        }

        private int CheckCost(int[,] matrix)
        {
            int cost = 0;

            int rowsOfmatrix = matrix.GetLength(0);
            int columnsOfMatrix = matrix.GetLength(1);

            for (int m = 0; m < columnsOfMatrix; m++)
            {
                for (int n = 0; n < rowsOfmatrix; n++)
                {
                    cost += matrix[m, n];
                    // suma kosztów wszystkich krawedzi
                }
            }

            return cost;
        }

        private void CreateGraph(int[,] matrix)
        {
            Graph graph = new Graph("graph");

            int rowsOfmatrix = matrix.GetLength(0);
            int columnsOfMatrix = matrix.GetLength(1);


            for (int m = 0; m < columnsOfMatrix; m++)
            {
                for (int n = 0; n < rowsOfmatrix; n++)
                {
                    if (matrix[m, n] != 0)
                    {
                        graph.AddEdge(m.ToString(), matrix[m, n].ToString(), n.ToString()).Attr.ArrowheadAtSource = ArrowStyle.None;
                    }
                }
            }

            this.gViewer.Graph = graph;

        }

        private void CreateGraph(int[,] matrix, int[,] newMatrix, bool onlyNew)
        {
            Graph graph = new Graph("graph");

            int rowsOfmatrix = matrix.GetLength(0);
            int columnsOfMatrix = matrix.GetLength(1);


            for (int m = 0; m < columnsOfMatrix; m++)
            {
                for (int n = 0; n < rowsOfmatrix; n++)
                {
                    if (matrix[m, n] != 0)
                    {
                        if (newMatrix[m, n] != 0)
                        {
                            Edge edge = (Edge)graph.AddEdge(m.ToString(), matrix[m, n].ToString(), n.ToString());
                            edge.Attr.Color = Microsoft.Msagl.Drawing.Color.Red;
                            edge.Attr.ArrowheadAtTarget = ArrowStyle.None;
                        }
                        else
                        {

                            if (onlyNew)
                            {
                                Edge edge = (Edge)graph.AddEdge(m.ToString(), n.ToString());
                                edge.Attr.Color = Microsoft.Msagl.Drawing.Color.Transparent;
                                edge.Attr.ArrowheadAtTarget = ArrowStyle.None;
                            }
                            else
                            {
                                graph.AddEdge(m.ToString(), matrix[m, n].ToString(), n.ToString()).Attr.ArrowheadAtTarget = ArrowStyle.None;
                            }
                        }
                    }
                }
            }

            this.gViewer.Graph = graph;

        }


    }
}
