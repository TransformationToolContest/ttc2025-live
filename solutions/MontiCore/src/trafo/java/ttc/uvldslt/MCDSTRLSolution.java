package ttc.uvldslt;

import de.monticore.ast.ASTNode;
import de.monticore.generating.GeneratorEngine;
import de.monticore.generating.GeneratorSetup;
import de.monticore.generating.templateengine.GlobalExtensionManagement;
import de.monticore.trafo.templating.PatternMatchingAdapter;
import de.se_rwth.commons.logging.Log;
import ttc.tr.UVLTFGenTool;
import ttc.tr.uvltr.UVLTRMill;
import ttc.uvl.UVLMill;
import ttc.uvl._ast.ASTFeatureModel;
import ttc2025.ISolution;

import java.io.File;
import java.io.IOException;
import java.util.Objects;
import java.util.function.Supplier;

public class MCDSTRLSolution implements ISolution {


  GeneratorEngine engine;
  GlobalExtensionManagement glex;

  @Override
  public void initialize() throws Exception {
    Log.init();
    UVLTRMill.init();
    UVLMill.init();
    this.glex = new GlobalExtensionManagement();
    GeneratorSetup setup = new GeneratorSetup();
    setup.setGlex(glex);
    setup.setTracing(false);
    this.engine = new GeneratorEngine(setup);
    var tool = new UVLTFGenTool();
    glex.setGlobalValue("printer", new Printer());
    glex.setGlobalValue("pm", new PatternMatchingAdapter<>(
            t -> UVLTRMill.parser().parse_String(t),
            tool::checkCoCos,
            tool::createODRule, () -> new dslaccessor.UVLAccessor()
    ));
  }

  public static class Printer {
    public String prettyPrint(ASTNode node) {
      return UVLMill.prettyPrint(node, false)
              .replace("<", "&lt;").replace(">", "&gt;").replace("&", "&amp;");
    }
  }


  String lastModelPath; // To ensure we only load one
  ASTFeatureModel astFeatureModel;
  @Override
  public void load(String modelPath, String model) {
    try {
      // create a graph model (serialized by the benchmark driver) or directly write a dot file, as you prefer
      this.astFeatureModel = UVLMill.parser().parse(modelPath).get();
      this.lastModelPath = modelPath;
    }catch (IOException e){
      throw new RuntimeException(e);
    }
  }


  @Override
  public Objects initial(String modelPath, String model, String targetPath) throws Exception {
    if (!Objects.equals(lastModelPath, modelPath)) {
      throw new IllegalStateException("Trying to use a model which was not loaded last");
    }
    // create a graph model (serialized by the benchmark driver) or directly write a dot file, as you prefer
    // Parse
    File targetFile = new File(targetPath).getAbsoluteFile();
    if (!targetFile.getParentFile().exists())
      targetFile.getParentFile().mkdirs();

    // Call template
    engine.generate("templates.01CreateDot.ftl", targetFile.toPath(), astFeatureModel);

    return null;
  }

  @Override
  public Supplier<Object> computeChanges(String modelPath, String model, int iteration, String targetPath) {
    // lines added here are not considered for time measurements, for instance to load changes
    load(modelPath, model);
    return () -> {
      // lines here are considered for time measurement
      try {
        return initial(modelPath, model, targetPath);
      } catch (Exception e) {
        throw new RuntimeException(e);
      }
    };
  }

  /**
   * Start the {@link MCDSTRLSolution} on the automotive01 model
   * @param args ignored
   */
  public static void main(String[] args) throws Exception {
    MCDSTRLSolution solution = new MCDSTRLSolution();
    solution.initialize();

    solution.load("models\\automotive01\\automotive01.uvl", "automotive");

    solution.initial("models\\automotive01\\automotive01.uvl", "automotive", "testOut/test.dot");
  }
}
