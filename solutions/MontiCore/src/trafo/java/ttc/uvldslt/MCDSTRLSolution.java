package ttc.uvldslt;

import de.monticore.ast.ASTNode;
import de.monticore.generating.GeneratorEngine;
import de.monticore.generating.GeneratorSetup;
import de.monticore.generating.templateengine.GlobalExtensionManagement;
import de.monticore.trafo.util.ReflectionDSLGrammarAccessor;
import de.monticore.trafo.templating.PatternMatchingAdapter;
import de.se_rwth.commons.logging.Log;
import ttc.tr.UVLTFGenTool;
import ttc.tr.uvltr.UVLTRMill;
import ttc.uvl.UVLMill;
import ttc2025.ISolution;

import java.io.File;
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
            tool::createODRule, () -> new ReflectionDSLGrammarAccessor<>(UVLMill.class)
    ));
  }

  public static class Printer {
    public String prettyPrint(ASTNode node) {
      return UVLMill.prettyPrint(node, false)
              .replace("<", "&lt;").replace(">", "&gt;").replace("&", "&amp;");
    }
  }


  @Override
  public void load(String modelPath, String model) {

  }

  @Override
  public Objects initial(String modelPath, String model, String targetPath) throws Exception {
    // create a graph model (serialized by the benchmark driver) or directly write a dot file, as you prefer
    // Parse
    var ast = UVLMill.parser().parse(modelPath);

    File targetFile = new File(targetPath).getAbsoluteFile();
    if (!targetFile.getParentFile().exists())
      targetFile.getParentFile().mkdirs();

    // Call template
    engine.generate("templates.01CreateDot.ftl", targetFile.toPath(), ast.get());

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
}
