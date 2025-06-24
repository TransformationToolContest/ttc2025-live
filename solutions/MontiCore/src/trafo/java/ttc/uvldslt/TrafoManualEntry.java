package ttc.uvldslt;

import de.monticore.trafo.GenericTrafo;

/**
 * Start the {@link MCDSTRLSolution} with some arguments
 */
public class TrafoManualEntry {
  public static void main(String[] args) throws Exception {
    GenericTrafo.debug=false;


    MCDSTRLSolution solution = new MCDSTRLSolution();
//    if (!f.getAbsoluteFile().exists())
//      throw new FileNotFoundException(f.getAbsolutePath());
    solution.initialize();

    solution.load("..\\..\\models\\automotive01\\automotive01.uvl", "automotive");


    solution.initial("models/automotive01/automotive01.uvl", "automotive", "testOut/test.dot");
  }
}
