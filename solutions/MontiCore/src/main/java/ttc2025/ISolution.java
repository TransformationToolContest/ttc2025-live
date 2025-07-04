package ttc2025;


import java.util.Objects;
import java.util.function.Supplier;

/**
 * Denotes a solution to the benchmark
 */
public interface ISolution {
    /**
     * initializes the solution
     */
    void initialize() throws Exception;

    /**
     * Loads the model
     *
     * @param modelPath the path of the model
     * @param model the name of the model
     */
    void load(String modelPath, String model) throws Exception;

    /**
     * Performs the initial transformation
     *
     * @param modelPath the path of the model
     * @param model the name of the model
     * @param targetPath the full path to the dot file, in case the solution directly creates dot files
     * @return a graph, in case an in-memory graph is returned, or null
     */
    Object initial(String modelPath, String model, String targetPath) throws Exception;

    /**
     * Calculates the change propagations
     *
     * @param modelPath the path of the model
     * @param model the name of the model
     * @param iteration the iteration
     * @param targetPath the full path to the dot file, in case the solution directly creates dot files
     * @return a lambda expression that will perform the change propagation
     */
    Supplier<Object> computeChanges(String modelPath, String model, int iteration, String targetPath) throws Exception;
}
