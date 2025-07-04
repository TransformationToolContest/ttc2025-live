/* (c) https://github.com/MontiCore/monticore */
package ttc.uvl;

import de.se_rwth.commons.logging.Log;
import ttc.uvl._ast.ASTFeatureModel;
import ttc2025.ISolution;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.OutputStreamWriter;
import java.util.Objects;
import java.util.function.Supplier;

/**
 * A solution using the {@link DotWriter} to print the DOT model by traversing
 * an UVL model
 */
public class MCSolution implements ISolution {

  protected DotWriter dotWriter;

  @Override
  public void initialize() throws Exception {
    Log.init();
    UVLMill.init();
    dotWriter = new DotWriter();
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

    File targetFile = new File(targetPath);
    if (!targetFile.getParentFile().exists())
      targetFile.getParentFile().mkdirs();

    var fs = new FileOutputStream(targetPath);
    var sw = new OutputStreamWriter(fs);
    sw.write(dotWriter.work(astFeatureModel));
    sw.close();
    fs.close();

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

  public static void main(String[] args) throws Exception {
    // An entry point to run this solution
    MCSolution solution = new MCSolution();
    solution.initialize();

    solution.load("models\\automotive01\\automotive01.uvl", "automotive");
    solution.initial("models\\automotive01\\automotive01.uvl", "automotive", new File("targetOut").getAbsolutePath());
  }
}
