package ttc.uvl;

import de.se_rwth.commons.logging.Log;
import de.se_rwth.commons.logging.LogStub;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;

import java.io.File;
import java.io.IOException;
import java.nio.file.Files;

public class UVLParseTest {
  @BeforeEach
  public void setUp() throws Exception {
    LogStub.initPlusLog();
    Log.setErrorHook(() -> {
      throw new AssertionError("Terminated");
    });
    Log.enableFailQuick(true);
    UVLMill.init();
  }

  @Test
  void testParser() throws Exception {
    try (var walker = Files.walk(new File("../../models").toPath())) {
      walker.forEach(p -> {
        if (!p.toFile().isFile() || !p.toFile().getName().endsWith(".uvl")) {
          return;
        }
        try {
          System.err.println(p);
          var ast = UVLMill.parser().parse(p.toFile().toString());

          System.err.println(p);
          Assertions.assertTrue(ast.isPresent());
        } catch (IOException e) {
          throw new RuntimeException(e);
        }
      });
    }
  }
}
