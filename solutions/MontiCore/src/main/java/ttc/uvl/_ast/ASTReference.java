/* (c) https://github.com/MontiCore/monticore */
package ttc.uvl._ast;

public class ASTReference extends ASTReferenceTOP {

  /**
   * Helper to turn references into Strings
   * @return the reference printed
   */
  public String asString() {
    StringBuilder builder = new StringBuilder();

    for (var id : this.getIdList()) {
      if (id.isPresentName())
        builder.append(id.getName());
      else
        builder.append(id.getID_NOT_STRICT());
      builder.append(".");
    }

    return builder.substring(0, builder.toString().length() - 1);
  }
}
