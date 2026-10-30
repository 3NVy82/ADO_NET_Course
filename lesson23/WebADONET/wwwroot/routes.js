// Фронтенд для Web API: вкладки для студентов, групп, преподавателей, предметов и оценок.
// Для каждой сущности описаны маршруты, поля формы и столбцы таблицы.

const entities = {
  students: {
    title: 'Студенты', single: 'студента', base: '/students', listUrl: '/students/all',
    fields: [
      { key: 'firstName', label: 'Имя', type: 'text', required: true },
      { key: 'lastName', label: 'Фамилия', type: 'text', required: true },
      { key: 'age', label: 'Возраст', type: 'number' },
      { key: 'groupId', label: 'Группа', type: 'select', source: 'groups', noneLabel: 'Без группы' }
    ],
    columns: [
      { key: 'id', label: 'Id' }, { key: 'firstName', label: 'Имя' }, { key: 'lastName', label: 'Фамилия' },
      { key: 'age', label: 'Возраст' }, { key: 'groupName', label: 'Группа' }
    ],
    optionLabel: r => `${r.firstName} ${r.lastName}`
  },
  groups: {
    title: 'Группы', single: 'группу', base: '/groups', listUrl: '/groups/all',
    fields: [{ key: 'name', label: 'Название группы', type: 'text', required: true }],
    columns: [{ key: 'id', label: 'Id' }, { key: 'name', label: 'Название' }],
    optionLabel: r => r.name
  },
  teachers: {
    title: 'Преподаватели', single: 'преподавателя', base: '/teachers', listUrl: '/teachers/all',
    fields: [
      { key: 'firstName', label: 'Имя', type: 'text', required: true },
      { key: 'lastName', label: 'Фамилия', type: 'text', required: true }
    ],
    columns: [{ key: 'id', label: 'Id' }, { key: 'firstName', label: 'Имя' }, { key: 'lastName', label: 'Фамилия' }],
    optionLabel: r => `${r.firstName} ${r.lastName}`
  },
  subjects: {
    title: 'Предметы', single: 'предмет', base: '/subjects', listUrl: '/subjects/all',
    fields: [
      { key: 'name', label: 'Название предмета', type: 'text', required: true },
      { key: 'teacherId', label: 'Преподаватель', type: 'select', source: 'teachers', noneLabel: 'Без преподавателя' }
    ],
    columns: [{ key: 'id', label: 'Id' }, { key: 'name', label: 'Название' }, { key: 'teacherName', label: 'Преподаватель' }],
    optionLabel: r => r.name
  },
  grades: {
    title: 'Оценки', single: 'оценку', base: '/grades', listUrl: '/grades/all',
    fields: [
      { key: 'studentId', label: 'Студент', type: 'select', source: 'students', required: true, noneLabel: 'Выберите студента' },
      { key: 'subjectId', label: 'Предмет', type: 'select', source: 'subjects', required: true, noneLabel: 'Выберите предмет' },
      { key: 'score', label: 'Оценка', type: 'number', required: true },
      { key: 'gradeDate', label: 'Дата', type: 'date', required: true }
    ],
    columns: [
      { key: 'id', label: 'Id' }, { key: 'studentName', label: 'Студент' }, { key: 'subjectName', label: 'Предмет' },
      { key: 'score', label: 'Оценка' }, { key: 'gradeDate', label: 'Дата' }
    ],
    optionLabel: r => r.name
  }
};

let current = 'students';
let editId = null;

const $ = id => document.getElementById(id);

function showMessage(text, isError) {
  const box = $('message');
  box.textContent = text;
  box.className = isError ? 'message error' : 'message ok';
}

async function readError(response) {
  let text = 'Статус ответа: ' + response.status;
  try { text = (await response.json()).message ?? text; } catch { }
  return text;
}

async function api(url, options) {
  const response = await fetch(url, options);
  if (!response.ok) throw new Error(await readError(response));
  return response.status === 204 ? null : response.json();
}

// ---------- Таблица ----------

function renderTable(rows, withActions) {
  const cfg = entities[current];
  if (rows.length === 0) {
    const p = document.createElement('p');
    p.className = 'empty';
    p.textContent = 'Записей пока нет';
    return p;
  }

  const table = document.createElement('table');
  const headRow = document.createElement('tr');
  for (const col of cfg.columns) {
    const th = document.createElement('th');
    th.textContent = col.label;
    headRow.appendChild(th);
  }
  if (withActions) {
    const th = document.createElement('th');
    th.textContent = 'Действия';
    headRow.appendChild(th);
  }
  const thead = document.createElement('thead');
  thead.appendChild(headRow);
  table.appendChild(thead);

  const tbody = document.createElement('tbody');
  for (const row of rows) {
    const tr = document.createElement('tr');
    for (const col of cfg.columns) {
      const td = document.createElement('td');
      td.textContent = row[col.key] ?? '';
      tr.appendChild(td);
    }
    if (withActions) {
      const td = document.createElement('td');
      const editBtn = document.createElement('button');
      editBtn.textContent = 'Изменить';
      editBtn.addEventListener('click', () => startEdit(row));
      const delBtn = document.createElement('button');
      delBtn.textContent = 'Удалить';
      delBtn.addEventListener('click', () => deleteRow(row));
      td.append(editBtn, ' ', delBtn);
      tr.appendChild(td);
    }
    tbody.appendChild(tr);
  }
  table.appendChild(tbody);
  return table;
}

async function loadList() {
  try {
    const rows = await api(entities[current].listUrl);
    $('list').replaceChildren(renderTable(rows, true));
  } catch (e) {
    showMessage('Ошибка загрузки данных: ' + e.message, true);
  }
}

// ---------- Форма ----------

async function buildForm() {
  const cfg = entities[current];
  const form = $('form');
  form.replaceChildren();
  $('formTitle').textContent = (editId === null ? 'Создать ' : 'Изменить ') + cfg.single + (editId === null ? '' : ` (Id ${editId})`);

  for (const field of cfg.fields) {
    let input;
    if (field.type === 'select') {
      input = document.createElement('select');
      const none = document.createElement('option');
      none.value = '';
      none.textContent = field.noneLabel ?? 'Не выбрано';
      input.appendChild(none);
      try {
        const source = entities[field.source];
        const items = await api(source.listUrl);
        for (const item of items) {
          const option = document.createElement('option');
          option.value = item.id;
          option.textContent = source.optionLabel(item);
          input.appendChild(option);
        }
      } catch (e) {
        showMessage('Ошибка загрузки списка: ' + e.message, true);
      }
    } else {
      input = document.createElement('input');
      input.type = field.type;
      input.placeholder = field.label;
      if (field.type === 'number') input.min = 0;
    }
    input.id = 'f_' + field.key;
    input.title = field.label;
    form.appendChild(input);
  }

  const saveBtn = document.createElement('button');
  saveBtn.textContent = editId === null ? 'Создать' : 'Сохранить';
  saveBtn.addEventListener('click', saveRow);
  form.appendChild(saveBtn);

  if (editId !== null) {
    const cancelBtn = document.createElement('button');
    cancelBtn.textContent = 'Отмена';
    cancelBtn.addEventListener('click', () => { editId = null; buildForm(); });
    form.appendChild(cancelBtn);
  }
}

function collectValues() {
  const cfg = entities[current];
  const values = {};
  for (const field of cfg.fields) {
    const raw = $('f_' + field.key).value;
    let value;
    if (field.type === 'number' || field.type === 'select') value = raw === '' ? null : Number(raw);
    else value = raw.trim();

    if (field.required && (value === null || value === '')) {
      throw new Error('Заполните поле: ' + field.label);
    }
    values[field.key] = value;
  }
  return values;
}

async function startEdit(row) {
  editId = row.id;
  await buildForm();
  for (const field of entities[current].fields) {
    const value = row[field.key];
    $('f_' + field.key).value = value === null || value === undefined ? '' : String(value).slice(0, field.type === 'date' ? 10 : undefined);
  }
  window.scrollTo({ top: 0, behavior: 'smooth' });
}

// ---------- CRUD ----------

// POST /{base} (CREATE) или PUT /{base}/{id} (UPDATE)
async function saveRow() {
  const cfg = entities[current];
  const isEdit = editId !== null;
  try {
    const body = collectValues();
    await api(isEdit ? `${cfg.base}/${editId}` : cfg.base, {
      method: isEdit ? 'PUT' : 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body)
    });
    showMessage(isEdit ? 'Запись обновлена' : 'Запись создана', false);
    editId = null;
    $('found').replaceChildren();
    await buildForm();
    await loadList();
  } catch (e) {
    showMessage((isEdit ? 'Ошибка обновления: ' : 'Ошибка создания: ') + e.message, true);
  }
}

// DELETE /{base}/{id}
async function deleteRow(row) {
  if (!confirm(`Удалить запись с Id ${row.id}?`)) return;
  try {
    await api(`${entities[current].base}/${row.id}`, { method: 'DELETE' });
    showMessage('Запись удалена', false);
    if (editId === row.id) editId = null;
    $('found').replaceChildren();
    await buildForm();
    await loadList();
  } catch (e) {
    showMessage('Ошибка удаления: ' + e.message, true);
  }
}

// GET /{base}/{id}
async function findRow() {
  const id = $('searchId').value;
  $('found').replaceChildren();
  if (!id) { showMessage('Введите Id', true); return; }
  try {
    const row = await api(`${entities[current].base}/${encodeURIComponent(id)}`);
    $('found').replaceChildren(renderTable([row], false));
    showMessage('', false);
  } catch (e) {
    showMessage(e.message, true);
  }
}

// ---------- Вкладки ----------

async function openTab(key) {
  current = key;
  editId = null;
  for (const button of $('tabs').children) {
    button.classList.toggle('active', button.dataset.key === key);
  }
  $('listTitle').textContent = 'Все: ' + entities[key].title.toLowerCase();
  $('found').replaceChildren();
  $('searchId').value = '';
  showMessage('', false);
  await buildForm();
  await loadList();
}

for (const key of Object.keys(entities)) {
  const button = document.createElement('button');
  button.textContent = entities[key].title;
  button.dataset.key = key;
  button.addEventListener('click', () => openTab(key));
  $('tabs').appendChild(button);
}

$('findBtn').addEventListener('click', findRow);
$('searchId').addEventListener('keydown', e => { if (e.key === 'Enter') findRow(); });

openTab('students');
