package ttc.uvldslt;

import ttc2025.ISolution;

import java.nio.file.Paths;

public class BenchmarkDriver {

    public static void main(String[] args) {
        solution = new MCDSTRLSolution(); // TODO: change this line
        try {
            Initialize();
            Load();
            Initial();
            for (var i = 2; i <= Sequences; i++) {
                Update(i);
            }
        } catch(Exception e) {
            e.printStackTrace();
        }
    }

    static String Model;
    static String ModelPath;
    static String Tool;
    static String RunIndex;
    static int Sequences;

    static long stopwatch;

    static ISolution solution;

//    static ResourceSet rs;

    public static void Initialize() throws Exception
    {
        stopwatch = System.nanoTime();

        Tool = System.getenv("Tool");
        Model = System.getenv("Model");
        RunIndex = System.getenv("RunIndex");
        ModelPath = System.getenv("ModelPath");
        Sequences = Integer.valueOf(System.getenv("Sequences"));
        solution.initialize();
        stopwatch = System.nanoTime() - stopwatch;
        Report(BenchmarkPhase.Initialization, -1, null);
    }

    public static void Load() throws Exception
    {
        stopwatch = System.nanoTime();
        solution.load(ModelPath, Model);
        stopwatch = System.nanoTime() - stopwatch;
        Report(BenchmarkPhase.Load, -1, String.valueOf(stopwatch));
    }


    public static void Initial() throws Exception
    {
        var dotPath = Paths.get(ModelPath, "..", "results", Model + "_" + Tool + ".dot");
        stopwatch = System.nanoTime();

        var result = solution.initial(ModelPath, Model, dotPath.normalize().toString());

        stopwatch = System.nanoTime() - stopwatch;
        
        exportIfNotNull(result, Paths.get(ModelPath, "..", "results", String.format("%s.xmi", Model)).normalize().toString());
        
        Report(BenchmarkPhase.Initial, -1, String.valueOf(stopwatch));
    }

    public static void Update(int iteration) throws Exception
    {
        var updatedPath = Paths.get(ModelPath, "..", String.format("%s_%02d.uvl", Model, iteration)).normalize().toString();
        var dotPath = Paths.get(ModelPath, "..", "results", String.format("%s_%02d_%s.dot", Model, iteration, Tool)).normalize().toString();
        var actualActions = solution.computeChanges(updatedPath, Model, iteration, dotPath);

        stopwatch = System.nanoTime();

        var graph = actualActions.get();

        stopwatch = System.nanoTime() - stopwatch;
        exportIfNotNull(graph, Paths.get(ModelPath, "..", "results", String.format("%s_%02d.xmi", Model, iteration)).normalize().toString());
                
        Report(BenchmarkPhase.Update, iteration, String.valueOf(stopwatch));
    }

    private static void exportIfNotNull(Object graph, String path) throws Exception {
        if (graph != null) {
            throw new IllegalStateException("TODO: dependency");
        }
    }

    public static void Report(BenchmarkPhase phase, int iteration, String result)
    {
        String iterationStr;
        if (iteration == -1) {
            iterationStr = "0";
        } else {
            iterationStr = Integer.toString(iteration);
        }
        System.out.println(String.format("%s;%s;%s;%s;%s;Time;%s", Tool, Model, RunIndex, iterationStr, phase.toString(), Long.toString(stopwatch)));
        if("true".equals(System.getenv("NoGC"))) {
            // nothing to do
        } else {
            Runtime.getRuntime().gc();
            Runtime.getRuntime().gc();
            Runtime.getRuntime().gc();
            Runtime.getRuntime().gc();
            Runtime.getRuntime().gc();
        }
        long memoryUsed = Runtime.getRuntime().totalMemory() - Runtime.getRuntime().freeMemory();
        System.out.println(String.format("%s;%s;%s;%s;%s;Memory;%s", Tool, Model, RunIndex, iterationStr, phase.toString(), Long.toString(memoryUsed)));
    }

    public enum BenchmarkPhase {
        Initialization,
        Load,
        Initial,
        Update
    }
}
