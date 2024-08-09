export default function BlockForm() {
	/* const { id } = useParams()
	const [title, setTitle] = useState('')
	const [block, setBlock] = useState({} as BlockInfo)
	const [tags, setTags] = useState([] as { label: string; value: number }[])

	const [propModal, setPropModal] = useState({
		open: false,
		old: '',
		key: 0,
		value: '',
	})
	const [tagModal, setTagModal] = useState({
		open: false,
		index: 0,
		type: 0,
		tagId: 0,
		name: '',
	})

	const [load, loading, errLoad] = useFetching(async () => {
		let resBlock = await axios.post('blocks/read', { id: id })
		let resTags = await axios.post('tags/list')
		setBlock(resBlock.data)
		setTitle(resBlock.data.Name)
		setTags(
			resTags.data.map((x: TagInfo) => ({ value: x.id, label: x.name })),
		)
	})

	const RelType = (type: number) => {
		switch (type) {
			case 0:
				return 'Входные'
			case 1:
				return 'Выходные'
			case 2:
				return 'Связанные'
			default:
				return 'Неизвестный вид отношений'
		}
	}

	const Tag = (tagId: number) => {
		try {
			let tag = tags.filter((x) => x.value === tagId)[0]
			return (
				<NavLink to={'/tags/' + tagId}>
					<Button>{tag.label}</Button>
				</NavLink>
			)
		} catch (e) {
			return 'не найден'
		}
	}

	const back = () => {
		router.navigate('/tags')
	}

	const [update] = useFetching(async () => {
		await axios.post('blocks/update', { block: block })
	})

	const [del] = useFetching(async () => {
		let res = await axios.post('blocks/delete', { Id: block.id })
		if (res.data.Done) back()
	})

	const [create] = useFetching(async () => {
		let res = await axios.post('blocks/create', { ParentId: block.id })
		if (res.data.Done) load()
	})

	const setProperty = () => {
		setPropModal({ open: false, old: '', key: 0, value: '' })
		setBlock({
			...block,
			properties: block.properties.map((p) =>
				p.id !== propModal.key
					? p
					: {
							...p,
							value: propModal.value,
					  },
			),
		})
	}

	const delProperty = () => {
		let props = block.Properties
		if (propModal.old !== '') delete props[propModal.old]
		setPropModal({ open: false, old: '', key: 0, value: '' })
		setBlock({ ...block, Properties: { ...props } })
	}

	const setTag = () => {
		let tag = {
			BlockId: block.Id,
			TagId: tagModal.tagId,
			Name: tagModal.name,
			Type: tagModal.type,
		} as Rel_Block_Type
		let relations = block.Tags
		if (tagModal.index < 0) relations.push(tag)
		else relations[tagModal.index] = tag
		setBlock({ ...block, Tags: relations })
		setTagModal({ open: false, index: 0, tagId: 0, name: '', type: 0 })
	}

	const delTag = () => {
		let relations = block.Tags
		if (tagModal.index > -1) relations.splice(tagModal.index, 1)
		setBlock({ ...block, Tags: relations })
		setTagModal({ open: false, index: 0, tagId: 0, name: '', type: 0 })
	}

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(() => {
		!!id && load()
	}, [id])

	return errLoad ? (
		<Navigate to='/offline' />
	) : loading ? (
		<i>загрузка...</i>
	) : (
		<>
			<Header
				left={
					<Button onClick={() => router.navigate('/blocks')}>
						Вернуться
					</Button>
				}
				right={
					<>
						<Popconfirm
							title='Вы уверены, что хотите удалить этот блок?'
							placement='bottom'
							onConfirm={del}
							okText='Да'
							cancelText='Нет'
						>
							<Button>Удалить</Button>
						</Popconfirm>
						<Button type='primary' onClick={update}>
							Сохранить
						</Button>
					</>
				}
			>
				Блок {title}
			</Header>
			<FormRow title='Имя'>
				<Input
					value={block.name}
					onChange={(e) =>
						setBlock({ ...block, name: e.target.value })
					}
				/>
			</FormRow>
			<FormRow title='Описание'>
				<Input.TextArea
					value={block.description ?? ''}
					onChange={(e) =>
						setBlock({ ...block, description: e.target.value })
					}
				/>
			</FormRow>
			<FormRow title='Родительский блок'>
				<Button
					onClick={() =>
						router.navigate(
							block.parent?.id === 0
								? '/blocks'
								: '/blocks/' + block.parent?.id,
						)
					}
				>
					Перейти
				</Button>
			</FormRow>
			<FormRow title='Вложенные блоки'>
				{block.children &&
					block.children.map((b) => (
						<div>
							<Button
								onClick={() =>
									router.navigate(
										b.id === 0
											? '/blocks'
											: '/blocks/' + b.id,
									)
								}
							>
								{b.name}
							</Button>
						</div>
					))}
				<Button onClick={create}>Создать</Button>
			</FormRow>
			<FormRow title='Свойства'>
				{block.properties.map((prop) => (
					<div
						key={prop.id}
						className='table-row'
						style={{
							display: 'grid',
							gridTemplateColumns: '1fr 2fr',
						}}
						onClick={() =>
							setPropModal({
								open: true,
								old: prop.value,
								key: prop.id,
								value: prop.value,
							})
						}
					>
						<span>{prop.name}</span>
						<span>{prop.name}</span>
					</div>
				))}
				<Button
					onClick={() =>
						setPropModal({
							open: true,
							old: '',
							key: 0,
							value: '',
						})
					}
				>
					Добавить свойство
				</Button>
			</FormRow>
			<FormRow title='Теги'>
				<div className='table'>
					<div className='table-header'>
						<span>Отношение</span>
						<span>Название</span>
						<span>Тег</span>
					</div>
					{block.tags &&
						block.tags.map((rel, i) => (
							<div
								key={i}
								className='table-row'
								onClick={() =>
									setTagModal({
										open: true,
										index: i,
										tagId: rel.TagId,
										type: rel.Type,
										name: rel.Name,
									})
								}
							>
								<span>{RelType(rel.Type)}</span>
								<span>{rel.Name}</span>
								<span>{Tag(rel.TagId)}</span>
							</div>
						))}
				</div>
				<div>
					<Button
						onClick={() =>
							setTagModal({
								open: true,
								index: -1,
								tagId: 0,
								type: 0,
								name: '',
							})
						}
					>
						Добавить тег
					</Button>
				</div>
			</FormRow>
			<Modal
				title={propModal.old}
				open={propModal.open}
				onCancel={() => setPropModal({ ...propModal, open: false })}
				footer={[
					<Button onClick={delProperty}>Удалить</Button>,
					<Button onClick={setProperty} type='primary'>
						Сохранить
					</Button>,
				]}
			>
				<FormRow title='Свойство'>
					<Input
						value={propModal.key}
						onChange={(e) =>
							setPropModal({ ...propModal, key: e.target.value })
						}
					/>
				</FormRow>
				<FormRow title='Значение'>
					<Input
						value={propModal.value}
						onChange={(e) =>
							setPropModal({
								...propModal,
								value: e.target.value,
							})
						}
					/>
				</FormRow>
			</Modal>
			<Modal
				title={
					tagModal.index < 0
						? 'Новый связанный тег'
						: 'Связанный тег #' + tagModal.index
				}
				open={tagModal.open}
				onCancel={() => setTagModal({ ...tagModal, open: false })}
				footer={[
					<Button onClick={delTag}>Удалить</Button>,
					<Button onClick={setTag} type='primary'>
						Сохранить
					</Button>,
				]}
			>
				<FormRow title='Тип отношений'>
					<Select
						value={tagModal.type}
						options={[
							{ value: 0, label: 'Входное значение' },
							{ value: 1, label: 'Выходное значение' },
							{ value: 2, label: 'Связанное значение' },
						]}
						onChange={(v) => setTagModal({ ...tagModal, type: v })}
						style={{ width: '100%' }}
					/>
				</FormRow>
				<FormRow title='Название'>
					<Input
						value={tagModal.name}
						onChange={(e) =>
							setTagModal({ ...tagModal, name: e.target.value })
						}
					/>
				</FormRow>
				<FormRow title='Тег'>
					<Select
						value={tagModal.tagId}
						options={tags}
						onChange={(v) => setTagModal({ ...tagModal, tagId: v })}
						style={{ width: '100%' }}
					/>
				</FormRow>
			</Modal>
		</>
	) */

	return <></>
}
