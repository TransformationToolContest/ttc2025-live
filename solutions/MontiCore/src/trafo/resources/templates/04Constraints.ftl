<#-- @ftlvariable name="pm" type="de.monticore.trafo.templating.PatternMatchingAdapter"  -->
<#-- @ftlvariable name="ast" type="ttc.uvl._ast.ASTFeatureModel"  -->
<#if ast.isPresentConstraints()>
  subgraph cluster_constraints{
  label="Constraints" color="white" fontcolor="white"
  constraints [shape="box" color="#1e1e1e" label=<<table border="0" cellpadding="0" cellspacing="0" cellborder="0">
      <#list ast.getConstraints().getConstraintList() as c>
        <tr>
          <td align="left">
              ${printer.prettyPrint(c)}
          </td>
        </tr>
      </#list>
  </table>>]
  }
</#if>
