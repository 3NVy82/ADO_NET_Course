// Работа с маршрутами Web API

function renderRows(tableId, students) {
  const tbody = document.querySelector('#' + tableId + ' tbody');
  tbody.innerHTML = '';
  for (const s of students) {
    const tr = document.createElement('tr');
    for (const value of [s.id, s.firstName, s.lastName, s.groupName]) {
      const td = document.createElement('td');
      td.textContent = value ?? '';
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

// GET students/all
async function loadAll() {
  try {
    const response = await fetch('/students/all');
    if (!response.ok) throw new Error('Статус ответа: ' + response.status);
    renderRows('students', await response.json());
  } catch (e) {
    showMessage('Ошибка загрузки данных: ' + e.message, true);
  }
}

// GET groups/all -> заполняем выпадающий список
async function loadGroups() {
  try {
    const response = await fetch('/groups/all');
    if (!response.ok) throw new Error('Статус ответа: ' + response.status);
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
    if (response.status === 404) {
      showMessage((await response.json()).message, true);
      return;
    }
    if (!response.ok) throw new Error('Статус ответа: ' + response.status);
    renderRows('student', [await response.json()]);
    table.hidden = false;
    showMessage('', false);
  } catch (e) {
    showMessage('Ошибка загрузки данных: ' + e.message, true);
  }
}

// POST student -> CREATE
async function createStudent() {
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

  try {
    const response = await fetch('/student', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(student)
    });

    if (!response.ok) {
      let text = 'Статус ответа: ' + response.status;
      try { text = (await response.json()).message ?? text; } catch { }
      throw new Error(text);
    }

    const created = await response.json();
    showMessage(`Студент создан: ${created.firstName} ${created.lastName} (Id ${created.id})`, false);
    document.getElementById('createForm').reset();
    await loadAll();
  } catch (e) {
    showMessage('Ошибка создания: ' + e.message, true);
  }
}

document.getElementById('findBtn').addEventListener('click', findStudent);
document.getElementById('studentId').addEventListener('keydown', e => { if (e.key === 'Enter') findStudent(); });
document.getElementById('createBtn').addEventListener('click', createStudent);

loadGroups();
loadAll();
