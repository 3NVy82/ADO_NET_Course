// Работа с маршрутами Web API

let editId = null; // Id редактируемого студента (null = режим создания)

function renderRows(tableId, students, withActions) {
  const tbody = document.querySelector('#' + tableId + ' tbody');
  tbody.innerHTML = '';
  for (const s of students) {
    const tr = document.createElement('tr');
    for (const value of [s.id, s.firstName, s.lastName, s.groupName]) {
      const td = document.createElement('td');
      td.textContent = value ?? '';
      tr.appendChild(td);
    }
    if (withActions) {
      const td = document.createElement('td');
      const editBtn = document.createElement('button');
      editBtn.textContent = 'Изменить';
      editBtn.addEventListener('click', () => startEdit(s));
      const delBtn = document.createElement('button');
      delBtn.textContent = 'Удалить';
      delBtn.addEventListener('click', () => deleteStudent(s));
      td.append(editBtn, ' ', delBtn);
      tr.appendChild(td);
    }
    tbody.appendChild(tr);
  }
}

function showMessage(text, isError) {
  const box = document.getElementById('message');
  box.textContent = text;
  box.className = isError ? 'message error' : 'message ok';
}

async function readError(response) {
  let text = 'Статус ответа: ' + response.status;
  try { text = (await response.json()).message ?? text; } catch { }
  return text;
}

// GET students/all
async function loadAll() {
  try {
    const response = await fetch('/students/all');
    if (!response.ok) throw new Error(await readError(response));
    renderRows('students', await response.json(), true);
  } catch (e) {
    showMessage('Ошибка загрузки данных: ' + e.message, true);
  }
}

// GET groups/all -> заполняем выпадающий список
async function loadGroups() {
  try {
    const response = await fetch('/groups/all');
    if (!response.ok) throw new Error(await readError(response));
    const select = document.getElementById('groupId');
    for (const g of await response.json()) {
      const option = document.createElement('option');
      option.value = g.id;
      option.textContent = g.name;
      select.appendChild(option);
    }
  } catch (e) {
    showMessage('Ошибка загрузки групп: ' + e.message, true);
  }
}

// GET student/{id}
async function findStudent() {
  const id = document.getElementById('studentId').value;
  const table = document.getElementById('student');
  table.hidden = true;
  if (!id) { showMessage('Введите Id студента', true); return; }

  try {
    const response = await fetch('/student/' + encodeURIComponent(id));
    if (!response.ok) throw new Error(await readError(response));
    renderRows('student', [await response.json()], false);
    table.hidden = false;
    showMessage('', false);
  } catch (e) {
    showMessage(e.message, true);
  }
}

// Переключение формы в режим редактирования
function startEdit(student) {
  editId = student.id;
  document.getElementById('firstName').value = student.firstName;
  document.getElementById('lastName').value = student.lastName;
  document.getElementById('groupId').value = student.groupId ?? '';
  document.getElementById('formTitle').textContent = 'Изменить студента (Id ' + student.id + ')';
  document.getElementById('saveBtn').textContent = 'Сохранить';
  document.getElementById('cancelBtn').hidden = false;
  window.scrollTo({ top: 0, behavior: 'smooth' });
}

function resetForm() {
  editId = null;
  document.getElementById('createForm').reset();
  document.getElementById('formTitle').textContent = 'Создать студента';
  document.getElementById('saveBtn').textContent = 'Создать';
  document.getElementById('cancelBtn').hidden = true;
}

// POST student (CREATE) или PUT student/{id} (UPDATE)
async function saveStudent() {
  const firstName = document.getElementById('firstName').value.trim();
  const lastName = document.getElementById('lastName').value.trim();
  const groupValue = document.getElementById('groupId').value;

  if (!firstName || !lastName) {
    showMessage('Введите имя и фамилию', true);
    return;
  }

  const student = {
    firstName: firstName,
    lastName: lastName,
    groupId: groupValue ? Number(groupValue) : null
  };

  const isEdit = editId !== null;
  try {
    const response = await fetch(isEdit ? '/student/' + editId : '/student', {
      method: isEdit ? 'PUT' : 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(student)
    });
    if (!response.ok) throw new Error(await readError(response));

    const saved = await response.json();
    showMessage(`${isEdit ? 'Студент обновлён' : 'Студент создан'}: ${saved.firstName} ${saved.lastName} (Id ${saved.id})`, false);
    resetForm();
    document.getElementById('student').hidden = true;
    await loadAll();
  } catch (e) {
    showMessage((isEdit ? 'Ошибка обновления: ' : 'Ошибка создания: ') + e.message, true);
  }
}

// DELETE student/{id}
async function deleteStudent(student) {
  if (!confirm(`Удалить студента ${student.firstName} ${student.lastName}?`)) return;

  try {
    const response = await fetch('/student/' + student.id, { method: 'DELETE' });
    if (!response.ok) throw new Error(await readError(response));

    showMessage(`Студент удалён: ${student.firstName} ${student.lastName}`, false);
    if (editId === student.id) resetForm();
    document.getElementById('student').hidden = true;
    await loadAll();
  } catch (e) {
    showMessage('Ошибка удаления: ' + e.message, true);
  }
}

document.getElementById('findBtn').addEventListener('click', findStudent);
document.getElementById('studentId').addEventListener('keydown', e => { if (e.key === 'Enter') findStudent(); });
document.getElementById('saveBtn').addEventListener('click', saveStudent);
document.getElementById('cancelBtn').addEventListener('click', resetForm);

loadGroups();
loadAll();
