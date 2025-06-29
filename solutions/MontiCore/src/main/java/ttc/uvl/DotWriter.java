/* (c) https://github.com/MontiCore/monticore */
package ttc.uvl;

import ttc.uvl._ast.*;
import ttc.uvl._visitor.UVLHandler;
import ttc.uvl._visitor.UVLTraverser;

import java.util.Stack;

/**
 * TODO ABC
 */
public class DotWriter {
  protected String work(ASTFeatureModel ast) {
    var dispatcher = UVLMill.typeDispatcher();
    StringBuilder sb = new StringBuilder();
    sb.append("digraph FeatureModel {\n");
    sb.append("rankdir=\"TB\"\n");
    sb.append("newrank=true\n");
    sb.append("bgcolor=\"#1e1e1e\"\n");
    sb.append("edge [color=white]\n");
    sb.append("node [style=filled fontcolor=\"white\" fontname=\"Arial Unicode MS, Arial\"];\n");
    sb.append("\n");

    // features
    if (ast.isPresentFeatures()) {
      var feature = ast.getFeatures().getFeature();
      var traverser = UVLMill.traverser();
      Stack<ASTFeature> featureStack = new Stack<>();
      featureStack.push(feature);
      traverser.setUVLHandler(new UVLHandler() {
        UVLTraverser realThis;

        @Override
        public UVLTraverser getTraverser() {
          return realThis;
        }

        @Override
        public void setTraverser(UVLTraverser traverser) {
          realThis = traverser;
        }

        @Override
        public void traverse(ASTGroup node) {
          for (var subFeature : node.getGroupSpec().getFeatureList()) {
            featureStack.push(subFeature);
            subFeature.accept(getTraverser());
            featureStack.pop();
            sb.append(featureStack.peek().getRef().asString());
            sb.append(" -> ");
            sb.append(subFeature.getRef().asString());
            if (node.isAlt())
              sb.append("[arrowhead=\"none\", arrowtail=\"odot\", dir=\"both\"] \n");
            else if (node.isOr())
              sb.append("[arrowhead=\"none\", arrowtail=\"dot\", dir=\"both\"] \n");
            else if (node.getKind() == ASTConstantsUVL.MANDATORY)
              sb.append("[arrowhead=\"dot\", arrowtail=\"none\", dir=\"both\"] \n");
            else if (node.getKind() == ASTConstantsUVL.OPTIONAL)
              sb.append("[arrowhead=\"odot\", arrowtail=\"none\", dir=\"both\"] \n");
            else
              throw new IllegalStateException("Unknown feature group type");
          }

        }
      });
      for (var group : feature.getGroupList()) {
        group.accept(traverser);
      }
      sb.append(feature.getRef().asString());
      sb.append(" [fillcolor=\"#ABACEA\" tooltip=\"Cardinality: None\" shape=\"");
      // TODO: Actually model abstract in the ast?
      if (feature.isPresentAttributes() &&
              feature.getAttributes().getAttributeList().stream()
                      .filter(dispatcher::isUVLASTValueAttribute)
                      .anyMatch(a -> dispatcher.asUVLASTValueAttribute(a).getKey().equals("abstract"))) {
        sb.append("invhouse");
      } else {
        sb.append("box");
      }
      sb.append("\"]");
    }


    if (ast.isPresentConstraints()) {
      // constraints
      sb.append("subgraph cluster_constraints{ \n");
      sb.append("    label=\"Constraints\" color=\"white\" fontcolor=\"white\" \n");
      sb.append("    constraints [shape=\"box\" color=\"#1e1e1e\" label=<<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" cellborder=\"0\"> \n");
      sb.append("    ");
      var traverser = UVLMill.traverser();
      traverser.setUVLHandler(new UVLHandler() {
        UVLTraverser realThis;

        @Override
        public UVLTraverser getTraverser() {
          return realThis;
        }

        @Override
        public void setTraverser(UVLTraverser traverser) {
          realThis = traverser;
        }

        @Override
        public void traverse(ASTLiteralConstraint node) {
          sb.append(node.getReference().asString());
        }

        @Override
        public void traverse(ASTImplicationConstraint node) {
          node.getL().accept(getTraverser());
          sb.append(" =&gt;");
          node.getR().accept(getTraverser());
        }

        @Override
        public void traverse(ASTOrConstraint node) {
          node.getL().accept(getTraverser());
          sb.append(" | ");
          node.getR().accept(getTraverser());
        }

        @Override
        public void traverse(ASTNotConstraint node) {
          sb.append("!");
          node.getConstraint().accept(getTraverser());
        }

        @Override
        public void traverse(ASTAndConstraint node) {
          node.getL().accept(getTraverser());
          sb.append(" & ");
          node.getR().accept(getTraverser());
        }

        @Override
        public void traverse(ASTEquivalenceConstraint node) {
          node.getL().accept(getTraverser());
          sb.append(" &lt;=&gt; ");
          node.getR().accept(getTraverser());

        }
      });
      for (var c : ast.getConstraints().getConstraintList()) {
        sb.append("    <tr><td align=\"left\">");
        c.accept(traverser);
        sb.append("</td></tr>");
      }
      sb.append("</table>>]\n");
      sb.append("}\n");
    }

    sb.append("}\n");

    return sb.toString();
  }


}
