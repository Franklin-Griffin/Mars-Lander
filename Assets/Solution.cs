using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
class Solution : MonoBehaviour
{
    public int[] xPoints;
    public int[] yPoints;
    public int X;
    public int Y;
    public int hSpeed;
    public int vSpeed;
    public int fuel;
    public static int GEN_SIZE = 100;
    Organism C;
    int[,] points;
    int landID;
    
    void Start()
    {
        StartCoroutine("GameLoop");
    }
    IEnumerator GameLoop()
    {
        //Generate points
        points = new int[xPoints.Length, 2];
        landID = 0;
        for (int i = 0; i < xPoints.Length; i++)
        {
            points[i, 0] = xPoints[i];
            points[i, 1] = yPoints[i];
            if (i != 0 && yPoints[i] == yPoints[i - 1])
            {
                landID = i - 1;
            }
        }
        Stopwatch watch = new Stopwatch();
        watch.Start();
        Stopwatch fullWatch = new Stopwatch();
        fullWatch.Start();
        Stopwatch fullWatch1 = new Stopwatch();
        fullWatch1.Stop();
        Stopwatch fullWatch2 = new Stopwatch();
        fullWatch2.Start();
        int count = 1;
        Population pop = new Population();
        //UnityEngine.Debug.Log("Initial creation time: " + watch.ElapsedMilliseconds);
        watch.Reset();
        fullWatch2.Stop();
        Organism ans;
        int totalTurns = 0;
        System.Random rand = new System.Random();
        while (true)
        {
            watch.Start();
            fullWatch1.Start();
            pop.TestPopulation(points, X, Y, hSpeed, vSpeed, fuel, landID, gameObject);
            //UnityEngine.Debug.Log("Looped test time: " + watch.ElapsedMilliseconds + " gen " + (count + 1));
            fullWatch1.Stop();
            watch.Stop();
            watch.Reset();
            yield return new WaitForEndOfFrame();
            count++;
            //Create new Generation
            watch.Start();
            fullWatch2.Start();
            //Clear
            int cc = transform.childCount;
            for (int i = 0; i < cc; i++)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
            //Check success
            for (int i = 0; i < pop.c.Length; i++)
            {
                if (pop.c[i].score == -1)
                {
                    pop.c[i].Test(points, X, Y, hSpeed, vSpeed, fuel, i, landID, gameObject, out totalTurns, 0);
                    ans = pop.c[i];
                    goto done;
                }
            }
            //Sort by score
            Organism[] newList = Normalize(pop.c).OrderBy(item => item.score).ToArray();
            string a = "";
            int pc = 0;
            for (int i = 0; i < newList.Length; i++)
            {
                a += newList[i].score;
                a += " ";
                if (newList[i].condition == 1) {
                    pc++;
                }
            }
            UnityEngine.Debug.Log("Scores: " + a + "\n" + pc);
            for (int i = 0; i < newList.Length; i += 2)
            {
                if (i >= newList.Length - newList.Length / 5)
                {
                    //Eltism
                    newList[i] = pop.c[i];
                    newList[i + 1] = pop.c[i + 1];
                    continue;
                }
                //Pairs of Organisms
                float r = rand.Next(0, 101) / 100;
                float r2 = 1 - r;
                Chromosome[] child1 = new Chromosome[newList[0].Chromosomes.Length];
                Chromosome[] child2 = new Chromosome[newList[0].Chromosomes.Length];
                Organism p1 = Roulette(newList);
                Organism p2 = Roulette(newList);
                for (int j = 0; j < child1.Length; j++)
                {
                    //Crossover
                    child1[j] = new Chromosome((int)(p1.Chromosomes[j].rotate * r + p2.Chromosomes[j].rotate * r2), (int)(p1.Chromosomes[j].power * r + p2.Chromosomes[j].power * r2));
                    child2[j] = new Chromosome((int)(p1.Chromosomes[j].rotate * r2 + p2.Chromosomes[j].rotate * r), (int)(p1.Chromosomes[j].power * r2 + p2.Chromosomes[j].power * r));
                }
                newList[i] = new Organism(child1);
                newList[i + 1] = new Organism(child2);
            }
            int rotate, power;

            //Mutation                                            ^Eltism
            for (int i = 0; i < newList.Length - newList.Length / 5; i++)
            {
                for (int j = 0; j < newList[i].Chromosomes.Length; j++)
                {
                    if (rand.Next(0, 101) <= 3)
                    {
                        newList[i].Chromosomes[j] = new Chromosome(rand.Next(-15,16), out rotate, rand.Next(0,5), out power);
                    }
                    if (j != 0)
                    {
                        if (Math.Abs(newList[i].Chromosomes[j].rotate - newList[i].Chromosomes[j - 1].rotate) > 15 || Math.Abs(newList[i].Chromosomes[j].power - newList[i].Chromosomes[j - 1].power) > 1)
                        {
                            newList[i].Chromosomes[j] = new Chromosome(newList[i].Chromosomes[j - 1].rotate, out rotate, newList[i].Chromosomes[j - 1].power, out power);
                        }
                    }
                }
            }
            pop = new Population(newList);
            //UnityEngine.Debug.Log("Creation time: " + watch.ElapsedMilliseconds + " gen " + (count + 1));
            fullWatch2.Stop();
            watch.Reset();
        }
    done:
        UnityEngine.Debug.Log("Total Time: " + fullWatch.ElapsedMilliseconds);
        UnityEngine.Debug.Log("Total Test Time: " + fullWatch1.ElapsedMilliseconds);
        UnityEngine.Debug.Log("Total Creation Time: " + fullWatch2.ElapsedMilliseconds);
        UnityEngine.Debug.Log("Generations: " + count);
        C = ans;
        StartCoroutine("FinalTest");
        ;
    }
    static Organism Roulette(Organism[] c)
    {
        float rand = new System.Random().Next(0, 1001) / 1000;
        float temprand = rand;
        for (int i = c.Length - 1; i > -1; i--)
        {
            if (c[i].score > temprand && c[i].condition == 1)
            {
                return c[i];
            }
            temprand -= c[i].score;
        }
        temprand = rand;
        for (int i = c.Length - 1; i > -1; i--)
        {
            if (c[i].score > temprand)
            {
                return c[i];
            }
            temprand -= c[i].score;
        }
        return c[c.Length-1];
    }
    static Organism[] Normalize(Organism[] c)
    {
        float total = 1;
        for (int i = 0; i < c.Length; i++)
        {
            total += c[i].score;
        }
        for (int i = 0; i < c.Length; i++)
        {
            c[i].score /= total;
        }
        return c;
    }
    IEnumerator FinalTest()
    {
        yield return null;
        float x = X;
        float y = Y;
        float VSpeed = vSpeed;
        float HSpeed = hSpeed;
        int Fuel = fuel;
        GameObject visualRep = gameObject;
        visualRep.transform.position = new Vector3(X, Y, 0);
        int finali = 0;
        for (int i = 0; i < C.Chromosomes.Length; i++)
        {
            finali = i;
            C.Chromosomes[i].RunTurn(ref x, ref y, ref VSpeed, ref HSpeed, ref Fuel,0);
            visualRep.transform.position = new Vector3(x, y, 0);
            visualRep.transform.eulerAngles = new Vector3(0, 0, C.Chromosomes[i].rotate - 180);
            //Collision
            for (int j = 0; j < points.GetLength(0) - 1; j++)
            {
                if (x > points[j, 0] && x < points[j + 1, 0])
                {
                    if (points[j, 1] == points[j + 1, 1])
                    {
                        //Flat & Centered above segment, collision possible. Put segment in y=mx+b form
                        int rise = points[j, 1] - points[j + 1, 1];
                        int run = points[j, 0] - points[j + 1, 0];
                        float m = (100 * rise) / run;
                        //y = mx+b, identity to b=y-mx. Use one of the points
                        float B = points[j, 1] - (m / 100) * points[j, 0];
                        if (y < (m / 100) * x + B)
                        {
                            //Collision occured, sunk below line
                            visualRep.transform.eulerAngles = new Vector3(0, 0, 180);
                            goto done;
                        }
                    }
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
        done:;
    }

    class Population
    { //Generation
        public Organism[] c = new Organism[GEN_SIZE];
        public Population()
        {
            for (int i = 0; i < c.Length; i++)
            {
                c[i] = new Organism();
            }
        }
        public Population(Organism[] array)
        {
            c = array;
        }
        public void TestPopulation(int[,] points, int X, int Y, int hSpeed, int vSpeed, int fuel, int landID, GameObject gameOb)
        { //gives Organisms their score
            for (int i = 0; i < c.Length; i++)
            {
                int x;
                c[i].Test(points, X, Y, hSpeed, vSpeed, fuel, i, landID, gameOb, out x, i);
            }
        }
    }

    class Organism
    { //Rocket
        public Chromosome[] Chromosomes = new Chromosome[100];
        public float score;
        public int condition;
        public Organism()
        {
            int prevRot = 0;
            int prevPow = 0;
            for (int i = 0; i < Chromosomes.Length; i++)
            {
                int rotate;
                int power;
                Chromosomes[i] = new Chromosome(prevRot, out rotate, prevPow, out power);
                prevRot = rotate;
                prevPow = power;
            }
        }
        public Organism(Chromosome[] array)
        {
            Chromosomes = array;
        }
        public float Test(int[,] points, int AX, int AY, int hSpeed, int vSpeed, int fuel, int k, int landID, GameObject gameOb, out int totalTurns, int chrom)
        {
            float x = AX;
            float y = AY;
            float VSpeed = vSpeed;
            float HSpeed = hSpeed;
            int Fuel = fuel;
            GameObject ob = new GameObject();
            ob.transform.parent = gameOb.transform;
            LineRenderer visualRep = ob.AddComponent<LineRenderer>();
            visualRep.startWidth = 10;
            visualRep.endWidth = 10;
            visualRep.positionCount = 1;
            visualRep.SetPosition(0, new Vector3(AX, AY, 0));
            visualRep.gameObject.name = "Organism " + k;
            int finali = 0;
            totalTurns = 1;
            for (int i = 0; i < Chromosomes.Length; i++)
            {
                finali = i;
                Chromosomes[i].RunTurn(ref x, ref y, ref VSpeed, ref HSpeed, ref Fuel, chrom);
                visualRep.positionCount = i + 2;
                visualRep.SetPosition(i + 1, new Vector3(x, y, 0));
                //Check simple failure
                if (Fuel <= 0) //No Fuel
                {
                    condition = 0;
                    goto done;
                }
                if (x > 7000 || x < 0 || y > 3000 || y < 0)
                { //Out of Bounds
                    condition = 0;
                    goto done;
                }
                //Collision
                for (int j = 0; j < points.GetLength(0) - 1; j++)
                {
                    if (x >= points[j, 0] && x <= points[j + 1, 0])
                    {
                        if (j == landID)
                        {
                            //Flat & Centered above segment, collision possible. Put segment in y=mx+b form
                            int rise = points[j, 1] - points[j + 1, 1];
                            int run = points[j, 0] - points[j + 1, 0];
                            float m = (100 * rise) / run;
                            //y = mx+b, identity to b=y-mx. Use one of the points
                            float B = points[j, 1] - (m / 100) * points[j, 0];
                            if (y < (m / 100) * x + B)
                            {
                                //Collision occured, sunk below line
                                if (!(Chromosomes[i - 1].rotate < 15 && Chromosomes[i - 1].rotate > -15) || Math.Abs(VSpeed) > 40 || Math.Abs(HSpeed) > 20)
                                {
                                    condition = 1;
                                    goto done;
                                }
                                else
                                {
                                    condition = 2;
                                    Chromosomes[i].rotate = 0; //Vertical Landing
                                    goto done;
                                }
                            }
                        }
                        else
                        {
                            //Hilled & Centered above segment, collision possible. Put segment in y=mx+b form
                            int rise = points[j, 1] - points[j + 1, 1];
                            int run = points[j, 0] - points[j + 1, 0];
                            float m = (100 * rise) / run;
                            //y = mx+b, identity to b=y-mx. Use one of the points
                            float B = points[j, 1] - (m / 100) * points[j, 0];
                            if (y < (m / 100) * x + B)
                            {
                                //Collision occured, sunk below line
                                condition = 0;
                                goto done;
                            }
                        }
                    }
                }
                totalTurns++;
            }
        done:;
            if (condition == 0)
            {
                score = 1000 / (float)(Vector2.Distance(new Vector2(x, y), new Vector2((points[landID, 0] + points[landID + 1, 0]) / 2, points[landID, 1])));
                return score;
            }
            else if (condition == 1)
            {
                score = 5000;
                score -= 0.5f * (float)(Vector2.Distance(new Vector2(x, y), new Vector2((points[landID, 0] + points[landID + 1, 0]) / 2, points[landID, 1])));
                if (Math.Abs(VSpeed) > 40) {
                    score -= (int)(10 * Math.Abs(VSpeed));
                }
                if (Math.Abs(HSpeed) > 20) {
                    score -= 5 * (int)Math.Abs(HSpeed);
                }
                if (!(Chromosomes[finali - 1].rotate < 15 && Chromosomes[finali - 1].rotate > -15)) {
                    score -= 25 * (int)(Math.Abs(Chromosomes[finali - 1].rotate));
                }
                score *= 100;
                return score;
            }
            else if (condition == 2)
            {
                score = -1;
                return -1;
            }
            return 0;
        }
    }
    class Chromosome
    { //Game Turn
        public int rotate;
        public int power;
        public Chromosome(int prevRot, out int rotate, int prevPow, out int power)
        {
            System.Random rand = new System.Random();
            rotate = Mathf.Clamp(prevRot + rand.Next(-15, 16), -90, 90);
            //Bias
            int bias = Mathf.Clamp(rand.Next(-1, 2), -1, 1);
            power = Mathf.Clamp(prevPow + bias, 0, 4);
            this.rotate = rotate;
            this.power = power;
        }
        public Chromosome(int rotate1, int power1)
        {
            rotate = rotate1;
            power = power1;
        }
        public void RunTurn(ref float x, ref float y, ref float VSpeed, ref float HSpeed, ref int Fuel, int n)
        {
            // if (n == 99) {
            //     UnityEngine.Debug.Log(rotate);
            //     UnityEngine.Debug.Log(power);
            // }
            HSpeed -= (float)(4 * Math.Sin(rotate));
            VSpeed += (float)(4 * Math.Cos(rotate)) - 3.711f;
            x += HSpeed;
            y += VSpeed;
            Fuel -= 4;
        }
    }
}