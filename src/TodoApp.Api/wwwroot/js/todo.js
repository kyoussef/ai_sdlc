(() => {
  const qs = (sel, root = document) => root.querySelector(sel);
  const qsa = (sel, root = document) => Array.from(root.querySelectorAll(sel));

  const state = {
    page: 1,
    pageSize: 5,
    sort: 'createdAt',
    order: 'desc',
    total: 0,
    pageCount: 1,
    q: '',
    priorities: [],
    tags: []
  };

  const els = {
    addBtn: null,
    modal: null,
    modalTitle: null,
    search: null,
    clearSearch: null,
    priorityMenu: null,
    priorityDropdown: null,
    tagsMenu: null,
    tagsDropdown: null,
    sortBy: null,
    sortOrder: null,
    confirmModal: null,
    confirmTitle: null,
    confirmMessage: null,
    confirmDeleteBtn: null,
    form: null,
    title: null,
    titleError: null,
    description: null,
    dueDate: null,
    dueDateError: null,
    priority: null,
    tags: null,
    list: null,
    pagerPrev: null,
    pagerNext: null,
    pagerLabel: null,
    errorBanner: null,
    saveBtn: null,
    cancelBtn: null
  };

  let mode = 'create'; // 'create' | 'edit'
  let currentEditId = null;

  function openModal(title) {
    els.modalTitle.textContent = title || (mode === 'edit' ? 'Edit Task' : 'Add Task');
    const m = window.bootstrap ? window.bootstrap.Modal.getOrCreateInstance(els.modal) : null;
    if (m) m.show();
    setTimeout(() => els.title.focus(), 150);
  }

  function closeModal() {
    const m = window.bootstrap ? window.bootstrap.Modal.getOrCreateInstance(els.modal) : null;
    if (m) m.hide();
    els.addBtn.focus();
  }

  function openConfirm(title, message) {
    if (els.confirmTitle) els.confirmTitle.textContent = title || 'Confirm';
    if (els.confirmMessage) els.confirmMessage.textContent = message || '';
    const m = window.bootstrap ? window.bootstrap.Modal.getOrCreateInstance(els.confirmModal) : null;
    if (m) m.show();
  }

  function closeConfirm() {
    const m = window.bootstrap ? window.bootstrap.Modal.getOrCreateInstance(els.confirmModal) : null;
    if (m) m.hide();
  }

  function showErrorBanner(msg) {
    els.errorBanner.textContent = msg || 'Something went wrong.';
    els.errorBanner.classList.remove('d-none');
  }

  function hideErrorBanner() {
    els.errorBanner.textContent = '';
    els.errorBanner.classList.add('d-none');
  }

  function validateForm() {
    const title = els.title.value.trim();
    if (!title) {
      els.title.classList.add('is-invalid');
      els.titleError.textContent = 'Title is required';
      return false;
    }
    els.title.classList.remove('is-invalid');
    els.titleError.textContent = '';

    const dd = els.dueDate.value;
    if (!dd) {
      els.dueDate.classList.add('is-invalid');
      els.dueDateError.textContent = 'Due date is required';
      return false;
    }
    els.dueDate.classList.remove('is-invalid');
    els.dueDateError.textContent = '';
    return true;
  }

  function renderItem(item) {
    const tr = document.createElement('tr');
    tr.dataset.id = item.id;

    const mapPriorityDisplay = (p) => {
      switch (p) {
        case 2: return ['High', 'bg-danger'];
        case 1: return ['Med', 'bg-warning text-dark'];
        case 0: return ['Low', 'bg-secondary'];
        default: return [String(p), 'bg-secondary'];
      }
    };

    const isOverdue = () => {
      if (!item.dueDate || item.completed) return false;
      const today = new Date();
      const todayStart = new Date(today.getFullYear(), today.getMonth(), today.getDate());
      const due = new Date(item.dueDate + 'T00:00:00');
      return due < todayStart;
    };

    if (isOverdue()) tr.classList.add('table-danger');

    const tdTitle = document.createElement('td');
    const titleSpan = document.createElement('span');
    titleSpan.textContent = item.title;
    if (item.completed) titleSpan.style.textDecoration = 'line-through';
    titleSpan.className = 'task-title-text';
    tdTitle.appendChild(titleSpan);

    const tdPriority = document.createElement('td');
    const badge = document.createElement('span');
    const [pLabel, pClass] = mapPriorityDisplay(item.priority);
    badge.className = `badge ${pClass}`;
    badge.textContent = pLabel;
    tdPriority.appendChild(badge);

    const tdTags = document.createElement('td');
    const tags = Array.isArray(item.tags) ? item.tags.slice(0, 10) : [];
    if (tags.length === 0) {
      tdTags.textContent = '-';
    } else {
      tags.forEach(tag => {
        const b = document.createElement('span');
        b.className = 'badge bg-info text-dark me-1';
        b.textContent = tag;
        tdTags.appendChild(b);
      });
    }

    const tdDue = document.createElement('td');
    tdDue.textContent = item.dueDate || '-';

    const tdDays = document.createElement('td');
    const msPerDay = 24 * 60 * 60 * 1000;
    if (item.dueDate) {
      const today = new Date();
      const todayStart = new Date(today.getFullYear(), today.getMonth(), today.getDate());
      const due = new Date(item.dueDate + 'T00:00:00');
      const diffDays = Math.round((due - todayStart) / msPerDay);
      tdDays.textContent = String(diffDays);
    } else {
      tdDays.textContent = '-';
    }

    const tdCompleted = document.createElement('td');
    const chk = document.createElement('input');
    chk.type = 'checkbox';
    chk.className = 'form-check-input';
    chk.checked = !!item.completed;
    chk.setAttribute('aria-label', 'Toggle completed');
    tdCompleted.appendChild(chk);

    const tdUpdated = document.createElement('td');
    const formatDateTime = (s) => {
      if (!s) return '-';
      const d = new Date(s);
      if (isNaN(d.getTime())) return '-';
      return d.toLocaleString();
    };
    tdUpdated.textContent = formatDateTime(item.updatedAt);

    tr.appendChild(tdTitle);
    tr.appendChild(tdPriority);
    tr.appendChild(tdTags);
    tr.appendChild(tdDue);
    tr.appendChild(tdDays);
    tr.appendChild(tdCompleted);
    tr.appendChild(tdUpdated);

    const tdActions = document.createElement('td');
    const editBtn = document.createElement('button');
    editBtn.type = 'button';
    editBtn.className = 'btn btn-sm btn-outline-primary me-2';
    editBtn.textContent = 'Edit';
    editBtn.addEventListener('click', () => { mode = 'edit'; currentEditId = item.id; fillFormFromItem(item); openModal('Edit Task'); els.form.onsubmit = saveEdit; });
    tdActions.appendChild(editBtn);

    const delBtn = document.createElement('button');
    delBtn.type = 'button';
    delBtn.className = 'btn btn-sm btn-outline-danger';
    delBtn.textContent = 'Delete';
    delBtn.addEventListener('click', () => {
      openConfirm('Delete Task', 'Are you sure you want to delete "' + item.title + '"?');
      if (els.confirmDeleteBtn) {
        els.confirmDeleteBtn.onclick = async () => {
          els.confirmDeleteBtn.disabled = true;
          try {
            const res = await fetch(`/api/tasks/${item.id}`, { method: 'DELETE' });
            if (res.status === 204) {
              tr.remove();
              await loadList();
              closeConfirm();
            } else if (res.status === 404) {
              showErrorBanner('Task not found (maybe already deleted).');
              closeConfirm();
              await loadList();
            } else {
              showErrorBanner('Failed to delete task.');
            }
          } catch (e) {
            console.error(e);
            showErrorBanner('Network error while deleting task.');
          } finally {
            els.confirmDeleteBtn.disabled = false;
          }
        };
      }
    });
    tdActions.appendChild(delBtn);
    tr.appendChild(tdActions);

    const updateOverdueClass = () => {
      // remove any previous styling
      tr.classList.remove('table-danger');
      if (!item.completed && item.dueDate) {
        const today = new Date();
        const todayStart = new Date(today.getFullYear(), today.getMonth(), today.getDate());
        const due = new Date(item.dueDate + 'T00:00:00');
        if (due < todayStart) tr.classList.add('table-danger');
      }
    };

    chk.addEventListener('change', async () => {
      const newVal = chk.checked;
      chk.disabled = true;
      try {
        const res = await fetch(`/api/tasks/${item.id}`, {
          method: 'PATCH',
          headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
          body: JSON.stringify({ completed: newVal })
        });
        if (!res.ok) {
          // revert UI if failed
          chk.checked = !newVal;
          return;
        }
        const updated = await res.json();
        item.completed = !!updated.completed;
        // strike-through title when completed
        const titleEl = tr.querySelector('.task-title-text');
        if (titleEl) titleEl.style.textDecoration = item.completed ? 'line-through' : '';
        updateOverdueClass();
      } catch (e) {
        console.error(e);
        chk.checked = !newVal;
      } finally {
        chk.disabled = false;
      }
    });

    updateOverdueClass();
    return tr;
  }

  function enterEditMode(tr, item) {
    if (tr.dataset.editing === 'true') return;
    tr.dataset.editing = 'true';
    const tdTitle = tr.children[0];
    tdTitle.innerHTML = '';
    const input = document.createElement('input');
    input.type = 'text';
    input.className = 'form-control form-control-sm';
    input.maxLength = 200;
    input.value = item.title;
    input.setAttribute('aria-label', 'Edit title');
    const error = document.createElement('div');
    error.className = 'invalid-feedback d-block';
    error.style.display = 'none';
    tdTitle.appendChild(input);
    tdTitle.appendChild(error);
    setTimeout(() => input.focus(), 0);

    const tdActions = tr.lastElementChild;
    tdActions.innerHTML = '';
    const saveBtn = document.createElement('button');
    saveBtn.type = 'button';
    saveBtn.className = 'btn btn-sm btn-success me-2';
    saveBtn.textContent = 'Save';
    const cancelBtn = document.createElement('button');
    cancelBtn.type = 'button';
    cancelBtn.className = 'btn btn-sm btn-secondary';
    cancelBtn.textContent = 'Cancel';
    tdActions.appendChild(saveBtn);
    tdActions.appendChild(cancelBtn);

    const validate = () => {
      const t = input.value.trim();
      if (!t) {
        error.textContent = 'Title is required';
        error.style.display = '';
        return false;
      }
      if (t.length > 200) {
        error.textContent = 'Title must be <= 200 characters';
        error.style.display = '';
        return false;
      }
      error.textContent = '';
      error.style.display = 'none';
      return true;
    };

    cancelBtn.addEventListener('click', () => {
      exitEditMode(tr, item.title);
    });

    saveBtn.addEventListener('click', async () => {
      if (!validate()) return;
      saveBtn.disabled = true;
      try {
        const newTitle = input.value.trim();
        const res = await fetch(`/api/tasks/${item.id}`, {
          method: 'PATCH',
          headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
          body: JSON.stringify({ title: newTitle })
        });
        if (!res.ok) {
          if (res.status === 400) {
            const problem = await res.json().catch(() => ({}));
            error.textContent = problem?.detail || 'Validation error';
            error.style.display = '';
          } else if (res.status === 404) {
            error.textContent = 'Task not found';
            error.style.display = '';
          } else if (res.status === 409) {
            error.textContent = 'Conflict: task was modified by someone else';
            error.style.display = '';
          } else {
            error.textContent = 'Failed to save changes';
            error.style.display = '';
          }
          return;
        }
        const updated = await res.json();
        item.title = updated.title;
        item.completed = updated.completed;
        exitEditMode(tr, item.title);
      } catch (e) {
        console.error(e);
        error.textContent = 'Network error while saving';
        error.style.display = '';
      } finally {
        saveBtn.disabled = false;
      }
    });
  }

  function exitEditMode(tr, titleText) {
    tr.dataset.editing = 'false';
    const tdTitle = tr.children[0];
    tdTitle.innerHTML = '';
    const span = document.createElement('span');
    span.className = 'task-title-text';
    span.textContent = titleText;
    tdTitle.appendChild(span);
    const tdActions = tr.lastElementChild;
    tdActions.innerHTML = '';
    const editBtn = document.createElement('button');
    editBtn.type = 'button';
    editBtn.className = 'btn btn-sm btn-outline-primary me-2';
    editBtn.textContent = 'Edit';
    editBtn.addEventListener('click', () => {
      const id = tr.dataset.id;
      // title text is the current cell content already, so pass minimal item
      enterEditMode(tr, { id, title: titleText });
    });
    tdActions.appendChild(editBtn);
  }

  function prependItem(item) {
    const row = renderItem(item);
    if (els.list.firstChild) {
      els.list.insertBefore(row, els.list.firstChild);
    } else {
      els.list.appendChild(row);
    }
  }

  function updatePager() {
    els.pagerLabel.textContent = `Page ${state.page} of ${state.pageCount}`;
    els.pagerPrev.disabled = state.page <= 1;
    els.pagerNext.disabled = state.page >= state.pageCount;
  }

  async function loadList() {
    try {
      hideErrorBanner();
      if (window && window.console) {
        console.log('[tasks] loadList', { page: state.page, pageSize: state.pageSize, sort: state.sort, order: state.order, q: state.q, priority: state.priority });
      }
      const qParam = state.q && state.q.trim().length > 0 ? `&q=${encodeURIComponent(state.q.trim())}` : '';
      const pParam = (state.priorities || []).map(p => `&priority=${encodeURIComponent(p)}`).join('');
      const tParam = (state.tags || []).map(t => `&tag=${encodeURIComponent(t)}`).join('');
      const url = `/api/tasks?page=${state.page}&pageSize=${state.pageSize}&sort=${state.sort}&order=${state.order}${qParam}${pParam}${tParam}`;
      const res = await fetch(url, { headers: { 'Accept': 'application/json' } });
      if (!res.ok) throw new Error(`Failed to load: ${res.status}`);
      const data = await res.json();
      state.total = data.total || 0;
      state.page = data.page || state.page;
      state.pageSize = data.pageSize || state.pageSize;
      state.pageCount = Math.max(1, Math.ceil(state.total / state.pageSize));

      els.list.innerHTML = '';
      const seenTags = new Set();
      (data.items || []).forEach(item => {
        const row = renderItem(item);
        els.list.appendChild(row);
        (item.tags || []).forEach(tag => seenTags.add(tag));
      });
      // populate tags dropdown from current page
      if (els.tagsMenu) {
        const selected = new Set(state.tags || []);
        els.tagsMenu.innerHTML = '';
        Array.from(seenTags).sort().forEach(tag => {
          const wrap = document.createElement('div');
          wrap.className = 'form-check';
          const inp = document.createElement('input');
          inp.type = 'checkbox'; inp.className = 'form-check-input'; inp.value = tag; inp.id = `tag_${tag}`;
          inp.checked = selected.has(tag);
          inp.addEventListener('change', onTagsChanged);
          const lab = document.createElement('label'); lab.className = 'form-check-label'; lab.setAttribute('for', inp.id); lab.textContent = tag;
          wrap.appendChild(inp); wrap.appendChild(lab);
          els.tagsMenu.appendChild(wrap);
        });
        updateDropdownLabel(els.tagsDropdown, 'Tags', state.tags);
      }
      updatePager();
    } catch (e) {
      showErrorBanner('Failed to load tasks.');
      console.error(e);
    }
  }

  // debounce helper to limit request frequency
  function debounce(fn, delay) {
    let t = null;
    return (...args) => {
      if (t) clearTimeout(t);
      t = setTimeout(() => fn(...args), delay);
    };
  }

  function updateDropdownLabel(btn, base, values) {
    if (!btn) return;
    if (!values || values.length === 0) { btn.textContent = base; return; }
    if (values.length <= 2) btn.textContent = `${base}: ${values.join(', ')}`; else btn.textContent = `${base} (${values.length})`;
  }

  function onPriorityChanged() {
    const checked = Array.from(els.priorityMenu?.querySelectorAll('input[type="checkbox"]:checked') || []).map(i => i.value);
    state.priorities = checked;
    updateDropdownLabel(els.priorityDropdown, 'Priority', state.priorities);
    state.page = 1;
    loadList();
  }

  function onTagsChanged() {
    const checked = Array.from(els.tagsMenu?.querySelectorAll('input[type="checkbox"]:checked') || []).map(i => i.value);
    state.tags = checked;
    updateDropdownLabel(els.tagsDropdown, 'Tags', state.tags);
    state.page = 1;
    loadList();
  }

  async function createTask(evt) {
    evt.preventDefault();
    hideErrorBanner();
    if (!validateForm()) return;
    const descVal = els.description.value.trim();
    const body = {
      title: els.title.value.trim(),
      description: descVal || undefined,
      dueDate: els.dueDate.value,
      priority: parseInt(els.priority.value, 10),
      tags: (els.tags.value || '')
        .split(',')
        .map(s => s.trim())
        .filter(s => s.length > 0)
        .slice(0, 10)
    };
    els.saveBtn.disabled = true;
    try {
      const res = await fetch('/api/tasks', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
        body: JSON.stringify(body)
      });
      if (res.status === 201) {
        const item = await res.json();
        // After create, refresh page 1 to reflect new total and ordering.
        state.page = 1;
        prependItem(item);
        loadList();
        els.form.reset();
        els.priority.value = '1';
        closeModal();
      } else if (res.status === 400) {
        const problem = await res.json().catch(() => ({}));
        const msg = problem?.detail || 'Validation error';
        showErrorBanner(msg);
      } else {
        showErrorBanner('Failed to create task.');
      }
    } catch (e) {
      console.error(e);
      showErrorBanner('Network error while creating task.');
    } finally {
      els.saveBtn.disabled = false;
    }
  }

  function fillFormFromItem(item) {
    els.title.value = item.title || '';
    els.description.value = item.description || '';
    els.dueDate.value = item.dueDate || '';
    els.priority.value = String(item.priority ?? 1);
    els.tags.value = (Array.isArray(item.tags) ? item.tags : []).join(', ');
  }

  async function saveEdit(evt) {
    evt.preventDefault();
    hideErrorBanner();
    if (!validateForm() || !currentEditId) return;
    const descVal = els.description.value.trim();
    const body = {
      title: els.title.value.trim(),
      description: descVal || undefined,
      dueDate: els.dueDate.value,
      priority: parseInt(els.priority.value, 10),
      tags: (els.tags.value || '')
        .split(',')
        .map(s => s.trim())
        .filter(s => s.length > 0)
        .slice(0, 10)
    };
    els.saveBtn.disabled = true;
    try {
      const res = await fetch(`/api/tasks/${currentEditId}`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
        body: JSON.stringify(body)
      });
      if (!res.ok) {
        if (res.status === 400) {
          const problem = await res.json().catch(() => ({}));
          showErrorBanner(problem?.detail || 'Validation error');
        } else if (res.status === 404) {
          showErrorBanner('Task not found');
        } else if (res.status === 409) {
          showErrorBanner('Conflict: task was modified by someone else');
        } else {
          showErrorBanner('Failed to save changes.');
        }
        return;
      }
      await loadList();
      closeModal();
    } catch (e) {
      console.error(e);
      showErrorBanner('Network error while saving changes.');
    } finally {
      els.saveBtn.disabled = false;
    }
  }

  function init() {
    const root = qs('#tasks-page');
    if (!root) return;
    els.addBtn = qs('#add-task-btn', root);
    els.modal = qs('#taskModal', document);
    els.modalTitle = qs('#taskModalLabel', document);
    els.form = qs('#create-task-form', root);
    els.title = qs('#title', root);
    els.titleError = qs('#title-error', root);
    els.description = qs('#description', root);
    els.dueDate = qs('#dueDate', root);
    els.dueDateError = qs('#dueDate-error', root);
    els.priority = qs('#priority', root);
    els.tags = qs('#tags', root);
    els.list = qs('#task-list', root);
    els.search = qs('#search', document);
    els.clearSearch = qs('#clear-search', document);
    els.priorityMenu = qs('#priorityMenu', document);
    els.priorityDropdown = qs('#priorityDropdown', document);
    els.tagsMenu = qs('#tagsMenu', document);
    els.tagsDropdown = qs('#tagsDropdown', document);
    els.confirmModal = qs('#confirmModal', document);
    els.confirmTitle = qs('#confirmModalLabel', document);
    els.confirmMessage = qs('#confirmMessage', document);
    els.confirmDeleteBtn = qs('#confirmDeleteBtn', document);
    els.pagerPrev = qs('#pager-prev', root);
    els.pagerNext = qs('#pager-next', root);
    els.pagerLabel = qs('#pager-label', root);
    els.errorBanner = qs('#error-banner', root);
    els.saveBtn = qs('#save-task-btn', root);
    els.cancelBtn = qs('#cancel-task-btn', root);
    els.sortBy = qs('#sortBy', document);
    els.sortOrder = qs('#sortOrder', document);

    els.addBtn.addEventListener('click', () => {
      mode = 'create';
      currentEditId = null;
      els.form.reset();
      els.priority.value = '1';
      openModal('Add Task');
      els.form.onsubmit = createTask;
    });
    els.cancelBtn.addEventListener('click', (e) => { e.preventDefault(); closeModal(); });
    els.form.addEventListener('submit', (e) => e.preventDefault());

    if (els.sortBy) {
      els.sortBy.value = state.sort;
      els.sortBy.addEventListener('change', () => {
        state.sort = els.sortBy.value;
        state.page = 1;
        loadList();
      });
    }
    if (els.sortOrder) {
      els.sortOrder.value = state.order;
      els.sortOrder.addEventListener('change', () => {
        state.order = els.sortOrder.value;
        state.page = 1;
        loadList();
      });
    }

    const debouncedSearch = debounce(() => { state.page = 1; loadList(); }, 300);
    if (els.search) {
      els.search.addEventListener('input', () => {
        state.q = els.search.value || '';
        debouncedSearch();
      });
    }
    if (els.clearSearch) {
      els.clearSearch.addEventListener('click', () => {
        state.sort = 'createdAt';
        state.order = 'desc';
        if (els.sortBy) els.sortBy.value = 'createdAt';
        if (els.sortOrder) els.sortOrder.value = 'desc';
        state.q = '';
        state.priorities = [];
        state.tags = [];
        if (els.search) els.search.value = '';
        if (els.priorityMenu) els.priorityMenu.querySelectorAll('input[type="checkbox"]').forEach(i => i.checked = false);
        if (els.tagsMenu) els.tagsMenu.querySelectorAll('input[type="checkbox"]').forEach(i => i.checked = false);
        updateDropdownLabel(els.priorityDropdown, 'Priority', state.priorities);
        updateDropdownLabel(els.tagsDropdown, 'Tags', state.tags);
        state.page = 1;
        loadList();
      });
    }

    if (els.priorityMenu) {
      els.priorityMenu.querySelectorAll('input[type="checkbox"]').forEach(inp => inp.addEventListener('change', onPriorityChanged));
    }

    els.pagerPrev.addEventListener('click', () => {
      if (state.page > 1) {
        state.page -= 1;
        loadList();
      }
    });
    els.pagerNext.addEventListener('click', () => {
      if (state.page < state.pageCount) {
        state.page += 1;
        loadList();
      }
    });

    loadList();
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    // DOM already parsed; run immediately
    init();
  }
})();


























