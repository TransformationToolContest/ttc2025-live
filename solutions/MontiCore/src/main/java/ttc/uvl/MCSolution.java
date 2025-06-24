/* (c) https://github.com/MontiCore/monticore */
package ttc.uvl;

import de.se_rwth.commons.logging.Log;
import ttc2025.ISolution;

import java.io.File;
import java.io.FileOutputStream;
import java.io.OutputStreamWriter;
import java.util.Objects;
import java.util.function.Supplier;

/**
 * A solution using the {@link DotWriter} to print the
 */
public class MCSolution implements ISolution {

  protected DotWriter dotWriter;

  @Override
  public void initialize() throws Exception {
    Log.init();
    UVLMill.init();
    dotWriter = new DotWriter();
  }


  @Override
  public void load(String modelPath, String model) {
  }

  @Override
  public Objects initial(String modelPath, String model, String targetPath) throws Exception {
    // create a graph model (serialized by the benchmark driver) or directly write a dot file, as you prefer
    var ast = UVLMill.parser().parse(modelPath);

    File targetFile = new File(targetPath);
    if (!targetFile.getParentFile().exists())
      targetFile.getParentFile().mkdirs();

    var fs = new FileOutputStream(targetPath);
    var sw = new OutputStreamWriter(fs);
    sw.write(dotWriter.work(ast.get()));
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
}
