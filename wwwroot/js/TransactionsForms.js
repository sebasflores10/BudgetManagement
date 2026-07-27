
// Código fue hecho en las siguientes clases para validar el DropDown cascada de
// Categorias:
// (Udemy): 143. Actualizando Transacciones - Parte 1
// (Udemy): 144. Actualizando Transacciones - Parte 2
// (Udemy): 145. Vista de Actualización de Transacciones[Adaptado]
////////////////////////////////////////////////////////////////////////////////////
function TransactionsInitForms(urlGetCategories){
    $("#operation_type_id").change(async function () {
        const selectedValue = $(this).val();

        const response = await fetch(urlGetCategories, {
            method: 'POST',
            body: selectedValue,
            headers: {
                'Content-Type': 'application/json'
            }
        });

        const json = await response.json();
        console.log(json);


        let options;
        if (json.length === 0) {
            options = `<option>No se encontraron aún registros de categorías</option>`;
        } else {
            options = json.map(category =>
                `<option value="${category.value}">${category.text}</option>`);
        }

        $("#category_id").html(options);
    });
}